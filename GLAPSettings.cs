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
        public static MonsterWorkload workload = MonsterWorkload.SinglePlaythrough;
        public static int baseItemShopCost = 100;
        public static int killQuestsPerMonster = 5; //allowed values: 3-10
        public static int experienceRate = 100;
        public static GoalType goalType = GoalType.CompleteStory;
        public static bool randomizeSounds = true;
        public static bool randomizeMusic = true;


        private static readonly float[] shopPriceBases = new float[]
        {
            5,
            6,
            4,
            20,
            7,
            6.5f,
            5.5f,
            1,
            8,
            4.4f,
            3.5f,
            5.6f,
            13,
            8.8f,
            7.7f,
            4.1f,
            6.2f,
            9.3f,
            10,
            3
        };

        public static int GetShopPrice(int i)
        {
            return (int)(shopPriceBases[i] / 2f * baseItemShopCost);
        }

        public static float ExperienceMultiplier { get{
                return experienceRate/100f; 
        } }

        public static void Set(Dictionary<string,object> data)
        {
            experienceRate = Convert.ToInt32(data["experience_rate"]);
            killQuestsPerMonster = Convert.ToInt32(data["kill_quests_per_monster"]);
            itemLevelType = (ItemLevelType)Convert.ToInt32(data["item_level_type"]);
            workload = (MonsterWorkload)Convert.ToInt32(data["monster_workload"]);
            baseItemShopCost = Convert.ToInt32(data["base_item_shop_price"]);
            goalType = (GoalType)Convert.ToInt32(data["goal"]);
            deathlink = Convert.ToInt32(data["death_link"]) == 1;
            randomizeSounds = Convert.ToInt32(data["randomize_sounds"]) == 1;
            randomizeMusic = Convert.ToInt32(data["randomize_music"]) == 1;
        }
    }
}
