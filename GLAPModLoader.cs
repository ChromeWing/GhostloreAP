using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using System.IO;

/* TODO:
4h * define an AP world for Ghostlore (almost done)
5h * interface current systems into the AP client package
2h * automatically clean up quest instances on detecting which checks were already done on loading into the game (since injected quests do not save)
1h * sync the item shop to know the current state of which checks were already purchased.
1h * fix the item shop erasing the potions etc.
2h * detect different goals reached
4h * complete the recipe system in this mod (loading currently unlocked recipes in the restaurant menu)
6h * add Chthonite and Astralite as checks
3h * successfully create a foolproof way of handing over the Chthonite and Astralite items to the player
2h * add chests as checks (up to 50)
2h * add coin rewards to the pool (from chests)
1h * make elite monsters count as 5 kills of that breed
2h * add kill count feed to the right half of the screen HUD
5h * connect successfully to a locally hosted multiworld
8h * create text field form on character creation that saves an Archipelago profile (this will allow multiple saved characters to have their own individually assigned multiworld)
2h * save granted items to the archipelago profile (in case someone were to create multiple characters under the same server)
total: * 50h
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
            singletons = new List<IGLAPSingleton>();
            InitSingletons();
            DebugShowMessage("Huh?");

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
            GLAPClient.EnsureExists();

            singletons.Add(ExtendedBindingManager.instance);
            singletons.Add(GLAPLocationManager.instance);
            singletons.Add(GLAPItemGiver.instance);
            singletons.Add(QuestFactory.instance);
            singletons.Add(CreatureCatalogLogger.instance);
            singletons.Add(GLAPNotification.instance);
            singletons.Add(ItemFactory.instance);
            singletons.Add(GLAPClient.instance);
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
            GLAPClient.instance.Connect("ChromeWingGL");

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

        public static void DebugShowMessage(string msg_,bool logIt_=true)
        {
            if(debugMsg == null)
            {
                var go = new GameObject("Archipelago");
                debugMsg = go.AddComponent<TestTextDrawer>();
            }

            if (logIt_)
            {
                GLAPNotification.instance.DisplayLog(msg_);
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
