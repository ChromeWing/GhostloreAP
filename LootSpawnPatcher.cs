using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GhostloreAP
{
    [HarmonyPatch(typeof(LootSpawn),nameof(LootSpawn.OnReachedGoal))]
    public class LootSpawnPatcher
    {
        static Item chthonite, astralite;

        static bool Prefix(LootTable ___lootTable)
        {
            if(chthonite == null)
            {
                InitItems();
            }
            if (___lootTable.LootEntries.Length > 0)
            {
                var item = ___lootTable.LootEntries[0].specificItem;
                if(item != null)
                {
                    if (item == chthonite)
                    {
                        GLAPClient.instance.CompleteChthoniteCheck();
                        return false;
                    }
                    if (item == astralite)
                    {
                        GLAPClient.instance.CompleteAstraliteCheck();
                        return false;
                    }
                }

                //TODO: add chest checks here

            }

            return true;
        }

        static void InitItems()
        {
            chthonite = ItemManager.instance.GetItemFromName("Chthonite");
            astralite = ItemManager.instance.GetItemFromName("Astralite");
        }
    }
}
