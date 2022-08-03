using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using System.IO;
using System.Reflection;

/* TODO:
4h * define an AP world for Ghostlore (almost done)
2h * detect different goals reached
3h * implement deathlink, enable/disable with one of the F1-F12 keys
total: * 9h
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

        private static List<string> masterLogs;

        public void OnCreated()
        {
            masterLogs = new List<string>();
            modInfo = ModManager.Mods.Find((mod) => mod.Name == "Archipelago");
            singletons = new List<IGLAPSingleton>();
            GLAPClient.EnsureExists();
            GLAPProfileManager.EnsureExists();
            GLAPNotification.EnsureExists();
            singletons.Add(GLAPClient.instance);
            singletons.Add(GLAPProfileManager.instance);
            singletons.Add(GLAPNotification.instance);

            DebugShowMessage("===============================");
            DebugShowMessage("===============================");
            DebugShowMessage(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
            DebugShowMessage("===============================");
            DebugShowMessage("===============================");


            harmony = new Harmony("com.chromewing.ghostlore-archipelago");
            SetExternalPatchActive(true);
        }

        private void SetExternalPatchActive(bool active_)
        {
            MethodBase method_ = typeof(NewGameMenu).GetMethod(nameof(NewGameMenu.OnNewGame), BindingFlags.Instance | BindingFlags.Public);
            MethodInfo patchMethod_ = CharacterCreationPatcher.GetPrefix(CharacterCreationPatcher.Prefix);


            MethodBase method2_ = typeof(OptionsMenuLoader).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo patchMethod2_ = CharacterCreationPatcher.GetPrefix2(CharacterCreationPatcher.Prefix2);


            MethodBase method3_ = typeof(LoadingManager).GetMethod(
                "LoadGame", 
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                CallingConventions.Any,
                new Type[] { typeof(SaveGameSummary) },
                null);
            MethodInfo patchMethod3_ = LoadGamePatcher.GetPostfix(LoadGamePatcher.Postfix);
            DebugShowMessage("methodNull?" + (method3_ == null));
            DebugShowMessage("patchNull?" + (patchMethod3_ == null));
            if (active_)
            {
                harmony.Patch(method_, new HarmonyMethod(patchMethod_));
                harmony.Patch(method2_, new HarmonyMethod(patchMethod2_));
                harmony.Patch(method3_, new HarmonyMethod(patchMethod3_));
            }
            else
            {
                harmony.Unpatch(method_, patchMethod_);
                harmony.Unpatch(method2_, patchMethod2_);
                harmony.Unpatch(method3_, patchMethod3_);
            }
            DebugShowMessage("External patching set to " + active_);
        }

        private void InitSingletons()
        {
            CreatureCatalogLogger.instance.Init();
            ItemFactory.instance.Init();
            ExtendedBindingManager.EnsureExists();
            GLAPLocationManager.EnsureExists();
            GLAPItemGiver.EnsureExists();
            RecipeStorage.EnsureExists();

            singletons.Add(ExtendedBindingManager.instance);
            singletons.Add(GLAPLocationManager.instance);
            singletons.Add(GLAPItemGiver.instance);
            singletons.Add(QuestFactory.instance);
            singletons.Add(CreatureCatalogLogger.instance);
            singletons.Add(ItemFactory.instance);
            singletons.Add(RecipeStorage.instance);
        }

        private void DisposeSingletons()
        {
            if(singletons == null) { return; }
            foreach(var singleton in singletons)
            {
                if(singleton != null)
                singleton.Cleanup();
            }
            singletons.Clear();
        }

        public void OnGameLoaded(LoadMode mode)
        {
            GameLoadedStart();

        }

        public async void GameLoadedStart()
        {
            if (!GLAPClient.instance.SessionMade)
            {
                await GLAPProfileManager.instance.ConnectLoadedProfile();
                if (!GLAPProfileManager.instance.ValidProfile)
                {
                    return;
                }
            }
            else
            {
                await GLAPClient.instance.WaitTillConnectionIsMade();
            }

            InitSingletons();

            ItemFactory.instance.CacheShopItemNames();
            GLAPLocationManager.instance.StartListeners();
            GLAPNotification.instance.Init();

            harmony.PatchAll();

            await ReloadQuests();



            GLAPClient.tryInstance?.ListenToItems();
        }

        private async Task ReloadQuests()
        {
            while (TimeManager.instance.IsPaused())
            {
                await Task.Yield();
            }
            SaveGame.GetSaveGame().Managers.Find((m) => { return m.GetType() == typeof(QuestManager.Data); }).Deserialize();
            QuestFactory.instance.FixAfterAwake();
        }

        public void OnGameUnloaded()
        {
            DebugShowMessage("OnGameUnloaded");
            if(GLAPLocationManager.instance != null)
            {
                GLAPLocationManager.instance.ShutdownListeners();
            }

        }

        public void OnReleased()
        {
            if(harmony != null)
            {
                SetExternalPatchActive(false);
                harmony.UnpatchAll();
            }

            DisposeSingletons();
        }


        public static void DebugShowMessage(string msg_,bool logIt_=false)
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

            masterLogs.Add(debugMsg.DisplayMessage(msg_));
        }

        public static void SaveLog()
        {
            var stream = new FileStream(Path.Combine(LoadingManager.PersistantDataPath, "GLAP_Log.txt"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            using (StreamWriter writer = new StreamWriter(stream))
            {
                foreach(string log in masterLogs)
                {
                    writer.WriteLine(log);
                }
            }
            
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
