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
            thread = FindQuestProgressPanel();
        }


        public void Cleanup()
        {
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

            GLAPModLoader.DebugShowMessage("Adding message: "+message_);
            messages.Add(new MessageEvent(message_,onAppear));
            
        }

        private async Task RenderMessage(MessageEvent message_)
        {
            levelText.text = message_.text;
            glyphsText.text = "";
            skillpointText.text = "";
            animator.ResetTrigger("start");
            animator.SetTrigger("start");
            levelUpText.SetActive(false);
            clonedPanel.gameObject.SetActive(true);

            message_.onMessageAppear?.Invoke();

            await Task.Delay(3000);
            animator.ResetTrigger("start");

        }

        private async Task RenderMessageLoop()
        {
            renderMessageLoopToken = new CancellationTokenSource();
            while (!renderMessageLoopToken.IsCancellationRequested)
            {
                if(messages.Count > 0)
                {
                    GLAPModLoader.DebugShowMessage("Detected new message!");
                    await RenderMessage(messages[0]);
                    messages.RemoveAt(0);
                }

                await Task.Yield();
            }
        }

        private async Task FindQuestProgressPanel()
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

            clonedPanel = Instantiate(panel.gameObject,panel.transform.position,panel.transform.rotation,panel.transform.parent).GetComponent<LevelUpPanel>();

            clonedPanel.gameObject.transform.SetParent(panel.gameObject.transform.parent);

            levelText = Traverse.Create(clonedPanel).Field("levelText").GetValue<TMP_Text>();
            glyphsText = Traverse.Create(clonedPanel).Field("glyphs").GetValue<TMP_Text>();
            skillpointText = Traverse.Create(clonedPanel).Field("skillpoint").GetValue<TMP_Text>();
            levelText.fontSize = levelText.fontSize * .6f;
            levelTextTransform = levelText.GetComponent<RectTransform>();
            levelTextTransform.localPosition = levelTextTransform.localPosition+Vector3.up*22;
            levelTextTransform.sizeDelta = new Vector2(500,400);

            animator = Traverse.Create(clonedPanel).Field("animator").GetValue<Animator>();
            levelUpText = clonedPanel.transform.GetChild(1).gameObject;

            await RenderMessageLoop();
        }

    }
}
