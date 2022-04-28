using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GhostloreAP
{
    public enum MonsterWorkload
    {
        TEST,
        QuickPlaythrough,
        SinglePlaythrough,
        SomeGrinding,
        GrindingRequired
    }

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

        public static int GetQuestWorkload(MonsterWorkload workload_,int i)
        {
            int[] killCounts = { 5, 5, 5, 5, 5 };

            switch (workload_)
            {
                case MonsterWorkload.QuickPlaythrough:
                    killCounts = new int[] { 5, 5, 10, 10, 10 };
                    break;
                case MonsterWorkload.SinglePlaythrough:
                    killCounts = new int[] { 5, 10, 15, 20, 20 };
                    break;
                case MonsterWorkload.SomeGrinding:
                    killCounts = new int[] { 5, 10, 20, 30, 40 };
                    break;
                case MonsterWorkload.GrindingRequired:
                    killCounts = new int[] { 10, 20, 30, 40, 50 };
                    break;
            }

            return killCounts[i];
        }

        public static QuestFactory _inst;

        private List<Quest> _quests = new List<Quest>();
        
        public void Cleanup()
        {
            _quests.Clear();
        }

        public List<Quest> CreateAllAPQuests()
        {
            if(_quests!=null && _quests.Count > 0) { return _quests; }
            List<Quest> allAPQuests = new List<Quest>();
            foreach(var creature in CreatureCatalogLogger.instance.creatures)
            {
                allAPQuests.Add(CreateAPQuest(String.Format("AP: {0}", creature.CreatureDisplayName), creature, MonsterWorkload.QuickPlaythrough));
            }
            _quests = allAPQuests;
            return allAPQuests;
        }

        public Quest CreateAPQuest(string questName_,Creature creature_,MonsterWorkload workload_)
        {
            Quest q = new Quest();

            Traverse.Create(q).Field("questName").SetValue(questName_);
            Traverse.Create(q).Field("attributes").SetValue(QuestAttributes.AutoGiven);

            List<QuestStage> _stage = new List<QuestStage>();
            
            for(int i = 0; i < 5; i++)
            {
                _stage.Add(CreateStage(creature_,i,workload_));
            }

            Traverse.Create(q).Field("stages").SetValue(_stage.ToArray());

            return q;
        }

        private QuestStage CreateStage(Creature creature_,int i,MonsterWorkload workload_)
        {
            int requirement_ = GetQuestWorkload(workload_, i);
            string[] killDescriptor = {
                "Kill",
                "Eliminate",
                "Destroy",
                "Exterminate",
                "Vanquish"
            };

            QuestStage s = new QuestStage();
            Traverse.Create(s).Field("stageName").SetValue(string.Format("{0} {1} {2}", killDescriptor[i], requirement_, creature_.CreatureDisplayName));
            Traverse.Create(s).Field("rewards").SetValue(new QuestReward[0]);

            List<QuestRequirement> _requirements = new List<QuestRequirement>();
            QuestRequirement r = new QuestRequirement();
            ExtendedBindingManager.instance.RegisterAndSet<XQuestRequirement>(r, (xr) =>
            {
                xr.target = creature_;
                xr.killRequirement = requirement_;
                xr.killCount = 0;
            });
            Traverse.Create(r).Field("requirementType").SetValue(QuestRequirementType.MapProgress);
            Traverse.Create(r).Field("location").SetValue(MapManager.instance.DefaultTown);
            Traverse.Create(r).Field("value").SetValue("");
            _requirements.Add(r);

            Traverse.Create(s).Field("requirements").SetValue(_requirements.ToArray());
            Traverse.Create(s).Field("rewards").SetValue(new QuestReward[0]);
            Traverse.Create(s).Field("achievement").SetValue(AchievementType.None);

            return s;
        }
    }

    [HarmonyPatch(typeof(QuestManager),"Awake")]
    public class QuestManagerPatcher
    {
        static void Postfix(ref Quest[] ___quests, ref QuestInstance[] ___questInstances)
        {
            GLAPModLoader.DebugShowMessage("we're in it to win it");

            List<Quest> _quests = new List<Quest>(___quests);
            
            _quests = _quests.Concat(QuestFactory.instance.CreateAllAPQuests()).ToList();
            //GLAPModLoader.DebugShowMessage("changed quests from count "+___quests.Length+" to "+_quests.Count);
            ___quests = _quests.ToArray();
            var newCount = ((Quest[])Traverse.Create(QuestManager.instance).Field("quests").GetValue()).Length;
            //GLAPModLoader.DebugShowMessage("new quest count is 14? " + newCount);
            ___questInstances = (from c in ___quests
                                 select c.MakeInstance()).ToArray<QuestInstance>();

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
