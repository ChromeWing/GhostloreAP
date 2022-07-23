using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection;

namespace GhostloreAP
{

    public class GLAPProfile
    {
        public string slot_name { get; set; }
        public string ip { get; set; }
        public int port { get; set; }
        public string password { get; set; }
        public List<ItemClaimedFromLocation> items {get; set;}
    }

    public class ItemClaimedFromLocation
    {
        public long location;
        public int item;
        public ItemClaimedFromLocation(long location_,int item_)
        {
            location = location_;
            item = item_;
        }
    }

    public class GLAPProfileManager : Singleton<GLAPProfileManager>, IGLAPSingleton
    {
        private string loadedCharacterHash;

        private GLAPProfile selectedProfile;

        public bool ValidProfile { get
            {
                return selectedProfile != null;
            } }

        private string SelectedProfilePath { get
            {
                return Path.Combine(ProfilePath, "AP_"+loadedCharacterHash + ".json");
            } }
        
        private string ProfilePath = Path.Combine(LoadingManager.PersistantDataPath, "AP_Profiles");

        public void Cleanup()
        {
            selectedProfile = null;
            loadedCharacterHash = null;
        }

        public async Task ConnectLoadedProfile()
        {
            if (!ValidProfile) 
            {
                GLAPModLoader.DebugShowMessage("no valid profile!");    
                return;
            }
            await GLAPClient.instance.Connect(selectedProfile);
        }

        public void DisconnectAndSave()
        {
            GLAPModLoader.DebugShowMessage("DisconnectAndSave()");
            if (!ValidProfile || loadedCharacterHash==null) 
            {

                GLAPModLoader.DebugShowMessage("DisconnectAndSave() had Null profile!");
                return;
            }
            Save(loadedCharacterHash);
            GLAPClient.instance.Disconnect();
        }

        public void Load(string hash)
        {
            loadedCharacterHash = hash;
            if (!File.Exists(SelectedProfilePath))
            {
                if(selectedProfile == null)
                {
                    return;
                }
                Save(hash);
            }
            selectedProfile = JsonConvert.DeserializeObject<GLAPProfile>(File.ReadAllText(SelectedProfilePath));
        }

        public void Save(string hash)
        {
            if(selectedProfile == null) { return; }
            loadedCharacterHash = hash;
            if (!Directory.Exists(ProfilePath)) 
            {
                Directory.CreateDirectory(ProfilePath);
            }
            File.WriteAllText(SelectedProfilePath, JsonConvert.SerializeObject(selectedProfile, Formatting.Indented));

        }

        public void InitProfile(string slotName, string ip, int port, string password)
        {
            selectedProfile = new GLAPProfile();
            SetValue((p) =>
            {
                p.slot_name = slotName;
                p.ip = ip;
                p.port = port;
                p.password = password;
                p.items = new List<ItemClaimedFromLocation>();
            });
        }

        public void SetValue(System.Action<GLAPProfile> cb)
        {
            if (selectedProfile == null) { return; }
            cb(selectedProfile);
        }


        public bool ItemGranted(int item_,long location_) //returns false when the item granted was already granted.
        {
            if(selectedProfile == null) { return false; }
            bool result = true;
            SetValue((p) =>
            {
                if(p.items.Where(i => (i.item==item_ && i.location == location_)).Count() > 0)
                {
                    result = false;
                    return;
                }
                p.items.Add(new ItemClaimedFromLocation(location_,item_));
            });
            return result;
        }

        

    }


    [HarmonyPatch(typeof(LoadingManager), nameof(LoadingManager.QuickSave))]
    public class QuickSavePatcher
    {
        static void Postfix(LoadingManager __instance)
        {
            SaveGame save_ = SaveGame.GetSaveGame();
            MethodInfo getAllPCs_ = __instance.GetType().GetMethod(
                "GetAllPCs", 
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                CallingConventions.Any,
                new Type[] { typeof(SaveGame) },
                null);
            List<CharacterContainer.Data> data_ = (List < CharacterContainer.Data > )getAllPCs_.Invoke(__instance, new object[] { save_ });
            GLAPProfileManager.instance.Save(data_.FirstOrDefault().GetSaveGameName());
        }
    }

    [HarmonyPatch(typeof(LoadingManager), nameof(LoadingManager.SaveAndExitImpl))]
    public class SaveAndExitImplPatcher
    {
        static void Postfix()
        {
            GLAPProfileManager.instance.DisconnectAndSave();
        }
    }

    public class LoadGamePatcher
    {
        public static void Postfix(SaveGameSummary summary)
        {
            GLAPProfileManager.instance.Load(summary.FileName);
        }

        public static MethodInfo GetPostfix(System.Action<SaveGameSummary> action)
        {
            return action.Method;
        }
    }


}
