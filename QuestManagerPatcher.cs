using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace GhostloreAP
{
    

    public class QuestFactory : IGLAPSingleton
    {
        public static QuestFactory instance
        {
            get
            {
                if (_inst == null)
                {
                    _inst = new QuestFactory();
                }
                return _inst;
            }
        }

        public static int GetQuestWorkload(Creature creature_,MonsterWorkload workload_,int i)
        {
            int[] killCounts = { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 };

            switch (workload_)
            {
                case MonsterWorkload.QuickPlaythrough:
                    killCounts = new int[] { 5, 5, 10, 10, 10, 10, 10, 10, 10, 10 };
                    break;
                case MonsterWorkload.SinglePlaythrough:
                    killCounts = new int[] { 5, 10, 15, 20, 20, 20, 20, 20, 20, 20 };
                    break;
                case MonsterWorkload.SomeGrinding:
                    killCounts = new int[] { 5, 10, 20, 30, 40, 40, 40, 40, 40, 40 };
                    break;
                case MonsterWorkload.GrindingRequired:
                    killCounts = new int[] { 10, 20, 30, 40, 50, 50, 50, 50, 50, 50 };
                    break;
            }

            return Math.Max((int)Math.Ceiling(killCounts[i]*GetCreatureWorkloadMultiplier(creature_)),1);
        }

        private static float GetCreatureWorkloadMultiplier(Creature creature_)
        {
            //multiply bosses by zero to ensure they are only a 1-kill quest (workload will round it up to 1 automatically)
            switch (creature_.CreatureDisplayName)
            {
                //bosses:
                case "Ice Jinn":
                    return 0;
                case "Thunder Jinn":
                    return 0;
                case "Fire Jinn":
                    return 0;
                case "Rafflesia":
                    return 0;
                case "Mogui Summoner":
                    return 0;
                //regular monsters:
                case "Gui-Kia":
                    return 1.5f;
                case "Pontianak Tree":
                    return 0.5f;

            }
            return 1;
        }

        public static int GetQuestCountForCreature(Creature creature_)
        {
            switch (creature_.CreatureDisplayName)
            {
                case "Ice Jinn":
                    return 1;
                case "Thunder Jinn":
                    return 1;
                case "Fire Jinn":
                    return 1;
                case "Rafflesia":
                    return 1;
                case "Mogui Summoner":
                    return 1;
                    
            }

            return GLAPSettings.killQuestsPerMonster;
        }

        public static QuestFactory _inst;

        private List<Quest> _quests = new List<Quest>();

        private List<QuestInstance> _questInstances = new List<QuestInstance>();

        private bool _initializedQuestInstances = false;

        private GameLocation defaultLocation;

        public bool CheckInitializedQuestInstances()
        {
            if (!_initializedQuestInstances)
            {
                _initializedQuestInstances = true;
                return false;
            }
            return true;
        }

        public void ClearAPQuestInstances()
        {
            _questInstances.Clear();
        }
        public void AddAPQuestInstance(QuestInstance q_)
        {
            _questInstances.Add(q_);
        }

        public bool CompletedAllKillsForCreature(Creature creature)
        {
            GLAPModLoader.DebugShowMessage(">>>>about to test questInstances.");
            foreach (QuestInstance q in _questInstances)
            {
                GLAPModLoader.DebugShowMessage(">>>>we did a questinstance!!!!");
                GLAPModLoader.DebugShowMessage(">>>currentStage=" + q.CurrentStage + ", length-1=" + (q.Quest.Stages.Length - 1));
                if (q.CurrentStage < q.Quest.Stages.Length-1) { continue; }
                GLAPModLoader.DebugShowMessage(">>>>we passed a questinstance!!!!");
                XQuestInstance xq = ExtendedBindingManager.instance.GetExtended<XQuestInstance>(q);
                if(xq != null)
                {
                    GLAPModLoader.DebugShowMessage(">>>>we are in the matches!!!!");
                    if (xq.Matches(creature)) { return true; }
                }
                else
                {

                    GLAPModLoader.DebugShowMessage(">>>>it's null.....");
                }
            }
            return false;
        }
        
        public void Cleanup()
        {
            _initializedQuestInstances = false;
            _quests.Clear();
            _questInstances.Clear();
        }

        public List<Quest> CreateAllAPQuests()
        {
            if (defaultLocation==null) { CreateDefaultLocation(); }
            if(_quests!=null && _quests.Count > 0) { return _quests; }
            List<Quest> allAPQuests = new List<Quest>();
            foreach(var creature in CreatureCatalogLogger.instance.creatures)
            {
                allAPQuests.Add(CreateAPQuest(String.Format("AP: {0}", creature.CreatureDisplayName), creature, GLAPSettings.workload));
            }
            _quests = allAPQuests;
            return allAPQuests;
        }

        private void CreateDefaultLocation()
        {
            defaultLocation = new GameLocation();
            Traverse.Create(defaultLocation).Field("gameLocationName").SetValue("FakeLocation");
            Traverse.Create(defaultLocation).Field("attributes").SetValue(GameLocationAttributes.None);
            Traverse.Create(defaultLocation).Field("sceneName").SetValue("FakeLocation");
            Traverse.Create(defaultLocation).Field("maxObjectives").SetValue(0);
            Traverse.Create(defaultLocation).Field("creatures").SetValue(new MapCreatureSpawn[0]);
            Traverse.Create(defaultLocation).Field("defaultPortalPosition").SetValue(Vector3.zero);
            Traverse.Create(defaultLocation).Field("questRequirement").SetValue(null);
            Traverse.Create(defaultLocation).Field("backgroundMusic").SetValue("");
            Traverse.Create(defaultLocation).Field("ambientMusic").SetValue("");
            Traverse.Create(defaultLocation).Field("startingLevel").SetValue(0);
            Traverse.Create(defaultLocation).Field("musicOverrides").SetValue(new GameLocationMusicOverride[0]);
        }

        public Quest CreateAPQuest(string questName_,Creature creature_,MonsterWorkload workload_)
        {
            Quest q = new Quest();

            Traverse.Create(q).Field("questName").SetValue(questName_);
            Traverse.Create(q).Field("attributes").SetValue(QuestAttributes.AutoGiven);

            List<QuestStage> _stage = new List<QuestStage>();
            
            for(int i = 0; i < GetQuestCountForCreature(creature_); i++)
            {
                _stage.Add(CreateStage(creature_,i,workload_));
            }

            _stage.Add(CreateStage(creature_, -1, workload_));

            Traverse.Create(q).Field("stages").SetValue(_stage.ToArray());

            ExtendedBindingManager.instance.RegisterAndSet<XQuest>(q, (xq) =>
            {
                xq.target = creature_;
            });

            return q;
        }

        private QuestStage CreateStage(Creature creature_,int i,MonsterWorkload workload_)
        {
            if (i < 0)
            {
                return ConstructStage(string.Format("Conquered {0}!", creature_.CreatureDisplayName),"", creature_, 9999999);
            }

            int requirement_ = GetQuestWorkload(creature_,workload_, i);
            string[] killDescriptor = {
                "Kill",
                "Slay",
                "Wipe Out",
                "Destroy",
                "Slaughter",
                "Vanquish",
                "Execute",
                "Annihilate",
                "Obliterate",
                "Exterminate"
            };

            string killText = killDescriptor[i];

            if (GetQuestCountForCreature(creature_) == 1)
            {
                killText = "Defeat";
            }

            return ConstructStage(
                string.Format("{0} {1} {2}", killText, requirement_, creature_.CreatureDisplayName), 
                string.Format("{0} {1}", killText, creature_.CreatureDisplayName), 
                creature_,
                requirement_
            );
        }

        private QuestStage ConstructStage(string name_, string locationName_, Creature creature_, int requirement_)
        {
            QuestStage s = new QuestStage();
            Traverse.Create(s).Field("stageName").SetValue(name_);
            Traverse.Create(s).Field("rewards").SetValue(new QuestReward[0]);

            List<QuestRequirement> _requirements = new List<QuestRequirement>();
            QuestRequirement r = new QuestRequirement();
            ExtendedBindingManager.instance.RegisterAndSet<XQuestRequirement>(r, (xr) =>
            {
                xr.locationName = locationName_;
                xr.target = creature_;
                xr.killRequirement = requirement_;
                xr.killCount = 0;
                
            });
            Traverse.Create(r).Field("requirementType").SetValue(QuestRequirementType.MapProgress);

            Traverse.Create(r).Field("location").SetValue(defaultLocation);
            Traverse.Create(r).Field("value").SetValue("");
            _requirements.Add(r);

            Traverse.Create(s).Field("requirements").SetValue(_requirements.ToArray());
            Traverse.Create(s).Field("rewards").SetValue(new QuestReward[0]);
            Traverse.Create(s).Field("achievement").SetValue(AchievementType.None);

            return s;
        }


        public void FixAfterAwake()
        {
            QuestManager.instance.OnQuestProgress(null);
        }
    }

    /*
    [HarmonyPatch(typeof(MapManager),nameof(MapManager.IsLocationCompleted))]
    public class MapManagerPatcher
    {
        static bool Prefix(GameLocation location, ref bool __result)
        {
            if(location == null)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
    */

    [HarmonyPatch(typeof(QuestManager), nameof(QuestManager.OnQuestProgress))]
    public class QuestManagerPatcher
    {
        static bool Prefix(QuestInstance questInstance, ref Quest[] ___quests, ref QuestInstance[] ___questInstances)
        {
            if (questInstance == null) { return false; }
            if(QuestFactory.instance==null || QuestFactory.instance.CheckInitializedQuestInstances()) { return true; }
            GLAPModLoader.DebugShowMessage("we're in it to win it");

            List<Quest> _quests = new List<Quest>(___quests);

            _quests = _quests.Concat(QuestFactory.instance.CreateAllAPQuests()).ToList();
            //GLAPModLoader.DebugShowMessage("changed quests from count "+___quests.Length+" to "+_quests.Count);
            ___quests = _quests.ToArray();
            var newCount = ((Quest[])Traverse.Create(QuestManager.instance).Field("quests").GetValue()).Length;
            //GLAPModLoader.DebugShowMessage("new quest count is 14? " + newCount);
            ___questInstances = (from c in ___quests
                                 select c.MakeInstance()).ToArray<QuestInstance>();

            QuestFactory.instance.ClearAPQuestInstances();
            foreach (QuestInstance q_ in ___questInstances)
            {
                XQuest xQuest = ExtendedBindingManager.instance.GetExtended<XQuest>(q_.Quest);
                if (xQuest != null)
                {
                    ExtendedBindingManager.instance.RegisterAndSet<XQuestInstance>(q_, (xqi) =>
                    {
                        xqi.target = xQuest.target;
                    });
                    QuestFactory.instance.AddAPQuestInstance(q_);
                }
            }

            return false;

        }
    }

    [HarmonyPatch(typeof(QuestManager.Data), nameof(QuestManager.Data.Deserialize))]
    public class QuestManagerDataPatcher
    {
        static void Prefix(ref QuestInstance[] ___questInstances)
        {
            List<QuestInstance> questInstances = new List<QuestInstance>(___questInstances);
            questInstances.RemoveAll(x => (x.Quest == null || x.Quest.Stages == null || x.Quest.QuestName.StartsWith("AP: ")));
            List<QuestInstance> apQuestInstances = (from c in QuestFactory.instance.CreateAllAPQuests() select c.MakeInstance()).ToList();
            questInstances = questInstances.Concat(apQuestInstances).ToList();

            ___questInstances = questInstances.ToArray();

            GLAPModLoader.DebugShowMessage("quests counts is " + ___questInstances.Length);
        }

    }
}
