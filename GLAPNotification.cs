using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using HarmonyLib;
using UnityEngine.UI;
using System.Threading;


namespace GhostloreAP
{
    public class GLAPNotification : Singleton<GLAPNotification>, IGLAPSingleton
    {
        private LevelUpPanel clonedPanel;
        private TMP_Text levelText;
        private TMP_Text glyphsText;
        private TMP_Text skillpointText;
        private Animator animator;
        private GameObject levelUpText;
        private RectTransform levelTextTransform;

        private Canvas canvas;

        private Logs log = new Logs("{1}> {0}\n", 10, 1, new Vector2(.5f, .5f), new Vector2(.01f, .4f), new Vector2(.4f, .85f), TextAlignmentOptions.TopLeft);
        private Logs killLog = new Logs("{1}{0}\n", 6, 1, new Vector2(.5f, .5f), new Vector2(.6f, .4f), new Vector2(.97f, .66f), TextAlignmentOptions.TopRight);




        private List<MessageEvent> messages = new List<MessageEvent>();

        public static readonly float LOG_FONT_SCALE = .8f;
        public static readonly float LEVEL_FONT_SCALE = .6f;

        private struct MessageEvent
        {
            public string text;
            public System.Action onMessageAppear;

            public MessageEvent(string text, Action onMessageAppear)
            {
                this.text = text;
                this.onMessageAppear = onMessageAppear;
            }

        }

        private class Logs
        {
            private string format;
            private int logLimit;
            private TMP_Text logText;
            private TMP_Text logTextShadow;

            private TMP_Text levelText;

            private List<string> logs = new List<string>();

            private float fontSize;
            private Vector2 pivot;
            private Vector2 anchorMin;
            private Vector2 anchorMax;
            private TextAlignmentOptions alignment;

            public Logs(string format_,int logLimit_,float fontSize_, Vector2 pivot_, Vector2 anchorMin_, Vector2 anchorMax_, TextAlignmentOptions alignment_)
            {
                format = format_;
                logLimit = logLimit_;
                fontSize = fontSize_;
                pivot = pivot_;
                anchorMin = anchorMin_;
                anchorMax = anchorMax_;
                alignment = alignment_;
            }

            public void SetLevelText(TMP_Text levelText_)
            {
                levelText = levelText_;
            }

            public void DisplayLog(string log_)
            {
                logs.Add(log_);
                RefreshLogs();
            }

            public void DisplayKillLog(string creature_,string log_)
            {
                for(int i = 0; i < logs.Count; i++)
                {
                    if (logs[i].StartsWith("Killed "+creature_))
                    {
                        logs[i] = log_;
                        RefreshLogs();
                        return;
                    }
                }
                logs.Add(log_);
                RefreshLogs();
            }

            public void RefreshLogs()
            {
                while (logs.Count > logLimit)
                {
                    logs.RemoveAt(0);
                }
                if(levelText == null) { return; }
                if (logText != null)
                {
                    string fullText_ = "";
                    for (int i = 0; i < logs.Count; i++)
                    {
                        fullText_ = String.Format(format, logs[i], fullText_);
                    }
                    logText.text = fullText_;
                    logTextShadow.text = logText.text;
                    if (!logText.gameObject.activeInHierarchy)
                    {
                        logText.gameObject.SetActive(true);
                        logTextShadow.gameObject.SetActive(true);
                        var p = logText.transform.parent;
                        while (p != null)
                        {
                            p.gameObject.SetActive(true);
                            p = p.parent;
                        }
                    }
                }
            }

            public bool ReloadLogs(Canvas canvas)
            {
                if (!canvas) { return false; }

                GLAPModLoader.DebugShowMessage(canvas.gameObject.name, false);

                if (logText == null)
                {
                    logText = InstantiateLogText(false,canvas);


                    logTextShadow = InstantiateLogText(true,canvas);

                    RefreshLogs();
                }

                return true;
            }

            private TMP_Text InstantiateLogText(bool shadow,Canvas canvas)
            {
                var t = Instantiate(levelText.gameObject, levelText.transform.position, levelText.transform.rotation, canvas.transform).GetComponent<TMP_Text>();
                t.transform.SetAsFirstSibling();
                var tTransform = t.GetComponent<RectTransform>();
                t.fontSize = t.fontSize * fontSize * LOG_FONT_SCALE;
                tTransform.pivot = pivot;
                tTransform.anchorMin = anchorMin;
                tTransform.anchorMax = anchorMax;
                t.alignment = alignment;

                Vector2 offset_ = new Vector2(0, 0);
                if (shadow)
                {
                    offset_ = new Vector2(.5f, -.5f);
                }

                tTransform.offsetMin = Vector2.zero + offset_;
                tTransform.offsetMax = Vector2.zero + offset_;
                tTransform.sizeDelta = Vector2.zero;

                if (shadow)
                {
                    t.color = Color.black;
                }

                return t;
            }


        }


        private Task thread;
        private CancellationTokenSource renderMessageLoopToken;

        public void Init()
        {
            thread = InitFlow();

        }


        public void Cleanup()
        {
            if (this == null) { return; }
            if(thread != null)
            {
                renderMessageLoopToken.Cancel();
            }
            if (clonedPanel != null)
            {
                GameObject.Destroy(clonedPanel.gameObject);
                levelText = null;
                animator = null;
            }

            GameObject.Destroy(gameObject);
        }

        public void DisplayMessage(string message_, System.Action onAppear=null)
        {
            messages.Add(new MessageEvent(message_,onAppear));
            
        }

        public void DisplayLog(string log_)
        {
            log.DisplayLog(log_);
        }

        public void DisplayKillLog(string creature_,string log_)
        {
            killLog.DisplayKillLog(creature_,log_);
        }

        public void DisplayKillLog(CharacterContainer killed_,int current_,int goal_)
        {
            var creature_ = killed_.Creature;
            DisplayKillLog(killed_.Creature.CreatureDisplayName(),string.Format("Killed {0} ({1}{2})",creature_.CreatureDisplayName(),current_,goal_>99999?(" Conquered"):("/"+goal_)));
        }

        

        private async Task RenderMessage(MessageEvent message_)
        {
            while (LostUIElements())
            {
                await CreateUIElements();
                await Task.Delay(500);
            }
            while (TimeManager.instance.IsPaused())
            {
                await Task.Yield();
            }
            await Task.Delay(100);
            while (TimeManager.instance.IsPaused())
            {
                await Task.Yield();
            }


            levelText.text = message_.text;
            glyphsText.text = "";
            skillpointText.text = "";
            animator.ResetTrigger("start");
            animator.SetTrigger("start");
            levelUpText.SetActive(false);
            clonedPanel.gameObject.SetActive(true);

            message_.onMessageAppear?.Invoke();

            await Task.Delay(3000);
            if(animator != null)
            {
                animator.ResetTrigger("start");
            }

        }

        private async Task RenderMessageLoop()
        {
            renderMessageLoopToken = new CancellationTokenSource();
            while (!renderMessageLoopToken.IsCancellationRequested)
            {
                if(LostUIElements())
                {
                    await CreateUIElements();
                }
                else
                {
                    log.RefreshLogs();
                    killLog.RefreshLogs();
                }

                if(messages.Count > 0)
                {
                    await RenderMessage(messages[0]);
                    messages.RemoveAt(0);
                }

                await Task.Delay(500);
            }
        }

        

        

        private async Task InitFlow()
        {
            await CreateUIElements();
            await RenderMessageLoop();
        }

        private bool LostUIElements()
        {
            return log == null || killLog == null;
        }

        private async Task CreateUIElements()
        {
            while (TimeManager.instance.IsPaused())
            {
                await Task.Yield();
            }
            LevelUpPanel panel = null;
            while (panel == null)
            {
                await Task.Delay(1000);
                panel = Component.FindObjectOfType<LevelUpPanel>(true);
                GLAPModLoader.DebugShowMessage("Trying to find QuestProgressPanel...");
            }
            GLAPModLoader.DebugShowMessage("QuestProgressPanel FOUND!");

            if(clonedPanel == null)
            {
                clonedPanel = Instantiate(panel.gameObject,panel.transform.position,panel.transform.rotation,panel.transform.parent).GetComponent<LevelUpPanel>();

                clonedPanel.gameObject.transform.SetParent(panel.gameObject.transform.parent);

                levelText = Traverse.Create(clonedPanel).Field("levelText").GetValue<TMP_Text>();
                glyphsText = Traverse.Create(clonedPanel).Field("glyphs").GetValue<TMP_Text>();
                skillpointText = Traverse.Create(clonedPanel).Field("skillpoint").GetValue<TMP_Text>();
                levelText.fontSize = levelText.fontSize * LEVEL_FONT_SCALE;
                levelTextTransform = levelText.GetComponent<RectTransform>();
                levelTextTransform.localPosition = levelTextTransform.localPosition+Vector3.up*22;
                levelTextTransform.sizeDelta = new Vector2(500, 400);

                log.SetLevelText(levelText);
                killLog.SetLevelText(levelText);

            }


            if (!LostUIElements())
            {
                canvas = null;
                foreach (var c in Component.FindObjectsOfType<Canvas>())
                {
                    if (c.gameObject.name == "UI")
                    {
                        canvas = c;
                    }
                }

                log.ReloadLogs(canvas);
                killLog.ReloadLogs(canvas);
            }

            animator = Traverse.Create(clonedPanel).Field("animator").GetValue<Animator>();
            levelUpText = clonedPanel.transform.GetChild(1).gameObject;

        }

    }
}
