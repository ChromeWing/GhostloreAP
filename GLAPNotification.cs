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

        private TMP_Text logText;
        private RectTransform logTextTransform;
        private List<string> logs = new List<string>();

        private List<MessageEvent> messages = new List<MessageEvent>();

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
            logs.Add(log_);
            RefreshLogs();

        }

        private void RefreshLogs()
        {
            while(logs.Count > 14)
            {
                logs.RemoveAt(0);
            }
            if (logText != null)
            {
                string fullText_ = "";
                for(int i = 0; i < logs.Count; i++)
                {
                    fullText_ = String.Format("{1}> {0}\n", logs[i],fullText_);
                }
                logText.text = fullText_;
                if (!logText.gameObject.activeInHierarchy)
                {
                    logText.gameObject.SetActive(true);
                    var p = logText.transform.parent;
                    while(p != null)
                    {
                        p.gameObject.SetActive(true);
                        p = p.parent;
                    }
                }
            }
        }

        private async Task RenderMessage(MessageEvent message_)
        {
            while (LostUIElements())
            {
                await CreateUIElements();
                await Task.Delay(500);
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
                    RefreshLogs();
                }

                if(messages.Count > 0)
                {
                    await RenderMessage(messages[0]);
                    messages.RemoveAt(0);
                }

                await Task.Delay(500);
            }
        }

        private bool ReloadLogs()
        {
            canvas = null;
            foreach (var c in Component.FindObjectsOfType<Canvas>())
            {
                if (c.gameObject.name == "UI")
                {
                    canvas = c;
                }
            }

            if (!canvas) { return false; }

            GLAPModLoader.DebugShowMessage(canvas.gameObject.name,false);

            if(logText == null)
            {
                logText = Instantiate(levelText.gameObject, levelText.transform.position, levelText.transform.rotation, canvas.transform).GetComponent<TMP_Text>();
                logText.transform.SetAsLastSibling();
                logTextTransform = logText.GetComponent<RectTransform>();
                logText.fontSize = logText.fontSize * .6f;
                RefreshLogs();
                logTextTransform.pivot = new Vector2(.5f, .5f);
                logTextTransform.anchorMin = new Vector2(.01f, .4f);
                logTextTransform.anchorMax = new Vector2(.4f, .85f);
                logText.alignment = TextAlignmentOptions.TopLeft;
                logTextTransform.offsetMin = Vector2.zero;
                logTextTransform.offsetMax = Vector2.zero;
                logTextTransform.sizeDelta = Vector2.zero;
                
            }

            return true;
        }

        private async Task InitFlow()
        {
            await CreateUIElements();
            await RenderMessageLoop();
        }

        private bool LostUIElements()
        {
            return logText == null;
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
                levelText.fontSize = levelText.fontSize * .6f;
                levelTextTransform = levelText.GetComponent<RectTransform>();
                levelTextTransform.localPosition = levelTextTransform.localPosition+Vector3.up*22;
                levelTextTransform.sizeDelta = new Vector2(500, 400);
            }

            ReloadLogs();

            animator = Traverse.Create(clonedPanel).Field("animator").GetValue<Animator>();
            levelUpText = clonedPanel.transform.GetChild(1).gameObject;

        }

    }
}
