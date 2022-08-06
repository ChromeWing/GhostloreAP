using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using FMODUnity;
using System.Reflection;
using FMOD.Studio;

namespace GhostloreAP
{
    [HarmonyPatch(typeof(RuntimeManager),nameof(RuntimeManager.PlayOneShot),typeof(Guid),typeof(Vector3))]
    public class AudioManagerPatcher
    {
        static bool Prefix(Guid guid, Vector3 position)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(AudioRandomizer.instance.GetSound(guid));
            eventInstance.set3DAttributes(position.To3DAttributes());
            eventInstance.start();
            eventInstance.release();
            return false;
        }
    }

    [HarmonyPatch(typeof(RuntimeManager), nameof(RuntimeManager.PlayOneShotAttached), typeof(Guid), typeof(GameObject))]
    public class AudioManagerPatcher2
    {
        static bool Prefix(Guid guid, GameObject gameObject)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(AudioRandomizer.instance.GetSound(guid));
            RuntimeManager.AttachInstanceToGameObject(eventInstance, gameObject.transform, gameObject.GetComponent<Rigidbody>());
            eventInstance.start();
            eventInstance.release();
            return false;
        }
    }

    [HarmonyPatch(typeof(MapManager),nameof(MapManager.ResetMusic))]
    public class AudioManagerPatcher3
    {
        static bool Prefix(MapManager __instance,string ___townMusic)
        {
            GameLocation gameLocation = AudioRandomizer.instance.GetMusicLocation(__instance.ActiveLocation.GameLocation);
            if ((gameLocation.Attributes & GameLocationAttributes.IsTown) != GameLocationAttributes.None && !string.IsNullOrEmpty(___townMusic))
            {
                Singleton<AudioManager>.instance.SetMusic(new string[]
                {
                ___townMusic,
                gameLocation.AmbientMusic
                });
                return false;
            }
            Singleton<AudioManager>.instance.SetMusic(new string[]
            {
            gameLocation.BackgroundMusic,
            gameLocation.AmbientMusic
            });

            return false;
        }
    }

    public class AudioRandomizer : Singleton<AudioRandomizer>, IGLAPSingleton
    {
        public void Cleanup()
        {

        }

        private Dictionary<Guid, Guid> shuffleSounds;
        private Dictionary<GameLocation, GameLocation> shuffleMusicLocations;

        private List<string> bannedSounds = new List<string> { //when a sound is banned, it will remain vanilla and not get shuffled.
            "player/step", 
            "environment",
            "level_up",
            "coin"
        };

        public void Init()
        {
            RandomizeSounds();
            RandomizeMusic();
        }

        public Guid GetSound(Guid sound)
        {
            if (!GLAPSettings.randomizeSounds) { return sound; }
            if(!shuffleSounds.ContainsKey(sound)) { return sound; }
            return shuffleSounds[sound];
        }

        public GameLocation GetMusicLocation(GameLocation location)
        {
            if (!GLAPSettings.randomizeMusic) { return location; }
            if (!shuffleMusicLocations.ContainsKey(location)) { return location; }
            return shuffleMusicLocations[location];
        }

        private void RandomizeSounds()
        {
            if (!GLAPSettings.randomizeSounds) { return; }
            shuffleSounds = new Dictionary<Guid, Guid>();
            List<Guid> sounds = new List<Guid>();
            Bank[] banklist;
            RuntimeManager.StudioSystem.getBankList(out banklist);
            foreach (Bank bank in banklist)
            {
                string path;
                bank.getPath(out path);
                if(path.ToLower().Contains("music") || path.ToLower().Contains("ambience"))
                {
                    continue;
                }
                EventDescription[] events_;
                bank.getEventList(out events_);
                for (int i = 0; i < events_.Length; i++)
                {
                    string eventPathName_;
                    events_[i].getPath(out eventPathName_);
                    bool doContinue = false;
                    foreach(string banned in bannedSounds)
                    {
                        if (eventPathName_.ToLower().Contains(banned)) { doContinue=true; break; }
                    }
                    if (doContinue) { continue; }
                    
                    GLAPModLoader.DebugShowMessage("unfiltered sound was:\"" + eventPathName_ + "\"");
                    Guid id;
                    if (events_[i].getID(out id) == FMOD.RESULT.OK)
                    {
                        if (!sounds.Contains(id))
                        {
                            sounds.Add(id);
                        }
                    }
                }
            }
            List<Guid> unshuffled = new List<Guid>();
            for (int i = 0; i < sounds.Count; i++)
            {
                unshuffled.Add(sounds[i]);
            }
            ShuffleWithSeed(sounds);
            for (int i = 0; i < unshuffled.Count; i++)
            {
                shuffleSounds.Add(unshuffled[i], sounds[i]);
            }
            GLAPModLoader.SaveLog();
        }

        private void RandomizeMusic()
        {
            if (!GLAPSettings.randomizeMusic) { return; }
            shuffleMusicLocations = new Dictionary<GameLocation, GameLocation>();

            List<GameLocation> locations = new List<GameLocation>();
            foreach (GameLocation location in MapManager.instance.AllLocations)
            {
                if (!locations.Contains(location))
                {
                    locations.Add(location);
                }
            }
            List<GameLocation> unshuffled = new List<GameLocation>();
            for (int i = 0; i < locations.Count; i++)
            {
                unshuffled.Add(locations[i]);
            }
            ShuffleWithSeed(locations);
            for (int i = 0; i < unshuffled.Count; i++)
            {
                shuffleMusicLocations.Add(unshuffled[i], locations[i]);
            }

        }

        private void ShuffleWithSeed<T>(List<T> list)
        {
            System.Random rng = new System.Random(GLAPClient.instance.Seed);
            int n = list.Count;

            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

        }

    }
}
