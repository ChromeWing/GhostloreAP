using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using System.IO;

/* TODO:
 * postfix NPCTrader.CheckInventory() to setup the archipelago itemshop.
 * postfix InventoryPanel.ReceiveDraggedItem() (the one that returns a bool) in order to detect a transaction occurred for a particular itemInstance in the archipelago shop.
 * ^^ you will want to grab the ItemData field inside itemInstance (Item type)
 * generate ExtendedItem extendedbinding for the Items in the archipelago shop in order to see who it's for.
 * 
 * 
 */

namespace GhostloreAP
{
    public class GLAPModLoader : IModLoader
    {
        private static TestTextDrawer debugMsg;

        private static Harmony harmony;

        private List<IGLAPSingleton> singletons;

        private static string DebugHierarchyPath = Path.Combine(LoadingManager.PersistantDataPath, "debug-hierarchy.txt");

        public static ModSummary modInfo;

        public void OnCreated()
        {
            modInfo = ModManager.Mods.Find((mod) => mod.Name == "Archipelago");
            DebugShowMessage("Huh?");
            singletons = new List<IGLAPSingleton>();
            InitSingletons();

            DebugShowMessage("Huh??");

            harmony = new Harmony("com.chromewing.ghostlore-archipelago");
            harmony.PatchAll();

            DebugShowMessage("Huh????");
        }

        private void InitSingletons()
        {
            CreatureCatalogLogger.instance.Init();
            ItemFactory.instance.Init();
            ExtendedBindingManager.EnsureExists();
            GLAPLocationManager.EnsureExists();
            GLAPItemGiver.EnsureExists();
            GLAPNotification.EnsureExists();

            singletons.Add(ExtendedBindingManager.instance);
            singletons.Add(GLAPLocationManager.instance);
            singletons.Add(GLAPItemGiver.instance);
            singletons.Add(QuestFactory.instance);
            singletons.Add(CreatureCatalogLogger.instance);
            singletons.Add(GLAPNotification.instance);
            singletons.Add(ItemFactory.instance);
        }

        private void DisposeSingletons()
        {
            foreach(var singleton in singletons)
            {
                if(singleton != null)
                singleton.Cleanup();
            }
            singletons.Clear();
        }

        public void OnGameLoaded(LoadMode mode)
        {
            GLAPLocationManager.instance.StartListeners();
            GLAPNotification.instance.Init();
            WelcomePlayer();
            
        }

        public void OnGameUnloaded()
        {
            GLAPLocationManager.instance.ShutdownListeners();
            Shutdown();
        }

        public void OnReleased()
        {
            harmony.UnpatchAll();

            DisposeSingletons();
            Shutdown();
        }

        private void Shutdown()
        {

        }

        private async void WelcomePlayer()
        {
           
            await DisplayMessageAsyncRoutine("Welcome to Archipelago!");



        }

        public static void DebugShowMessage(string msg_)
        {
            if(debugMsg == null)
            {
                var go = new GameObject("Archipelago");
                debugMsg = go.AddComponent<TestTextDrawer>();
            }
            debugMsg.DisplayMessage(msg_); 
        }

        private async Task DisplayMessageAsyncRoutine(string msg_)
        {
            await Task.Delay(1000);
            while (TimeManager.instance.IsPaused())
            {
                await Task.Yield();
            }
            await Task.Delay(500);
            GLAPNotification.instance.DisplayMessage("Welcome!");
            GLAPNotification.instance.DisplayMessage("Welcome! x2");
            GLAPNotification.instance.DisplayMessage("Welcome! x3");



        }


        public static void ReportHierarchy()
        {
            File.WriteAllText(DebugHierarchyPath, DebugRoot());
        }

        static string DebugRoot()
        {
            string result = "----- Debug root -----\n";
            foreach (GameObject go in GameObject.FindObjectsOfType(typeof(GameObject)))
            {
                if (go.transform.parent == null)
                {
                    result+= DebugDeeper(go.transform)+"\n";
                }
            }
            return result;
        }

        static string DebugDeeper(Transform transform)
        {
            int children = transform.childCount;
            string result = String.Format(
                "{0}-{1}>{2} has {3}\n",
                (transform.gameObject.activeSelf ? ("activeself") : ("notactiveself")),
                (transform.gameObject.activeSelf ? ("actInHier") : ("notActInHier")),
                transform.name,
                children
            );
            

            for (int child = 0; child < children; child++)
            {
                result+=DebugDeeper(transform.GetChild(child))+"\n";
            }
            return result;
        }


    }
}
