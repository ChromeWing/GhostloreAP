using Archipelago.MultiClient.Net.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public enum ItemLevelType
    {
        TiedToCharacterLevel,
        TiedToProgression
    }

    public enum GoalType
    {
        CompleteStory,
        ClearHellGate1,
        ClearHellGate3,
        ClearHellGate10
    }


    public static class GLAPSettings
    {
        public static bool deathlink = false;
        public static ItemLevelType itemLevelType = ItemLevelType.TiedToCharacterLevel;
        public static MonsterWorkload workload = MonsterWorkload.TEST;
        public static int baseItemShopCost = 100;
        public static int killQuestsPerMonster = 5; //allowed values: 3-10
        public static GoalType goalType = GoalType.CompleteStory;

        public static void Set(Dictionary<string,object> data)
        {
            GLAPModLoader.DebugShowMessage("SettingSettings!");
            GLAPModLoader.DebugShowMessage("SettingSettings!");
            GLAPModLoader.DebugShowMessage("SettingSettings!");
            killQuestsPerMonster = ((int)data["kill_quests_per_monster"]);
            GLAPModLoader.DebugShowMessage("SettingSettings!");
            itemLevelType = (ItemLevelType)((int)data["item_level_type"]);
            GLAPModLoader.DebugShowMessage("SettingSettings!");
            workload = (MonsterWorkload)((int)data["monster_workload"]);
            GLAPModLoader.DebugShowMessage("SettingSettings!");
            baseItemShopCost = ((int)data["base_item_shop_price"]);
            GLAPModLoader.DebugShowMessage("SettingSettings!");
            goalType = (GoalType)((int)data["goal"]);
            GLAPModLoader.DebugShowMessage("SettingSettings!");
            deathlink = (int)data["death_link"] == 1;
            GLAPModLoader.DebugShowMessage("SettingSettings!");
        }
    }
}
