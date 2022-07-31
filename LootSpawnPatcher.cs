using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GhostloreAP
{
    [HarmonyPatch(typeof(Chest),nameof(LootSpawn.OnReachedGoal))]
    public class LootSpawnPatcher
    {
        static Item chthonite, astralite;

        static bool Prefix(LootTable ___lootTable)
        {
            GLAPModLoader.DebugShowMessage("GOING TO OPEN A CHEST.");
            if (chthonite == null)
            {
                InitItems();
            }
            if (___lootTable.LootEntries.Length > 0)
            {
                GLAPModLoader.DebugShowMessage("THERE ARE LOOT ENTRIES.");
                var item = ___lootTable.LootEntries[0].specificItem;
                if(item != null)
                {
                    if (item == chthonite)
                    {
                        GLAPModLoader.DebugShowMessage("ABOUT TO DO CHTHONITE CHEST CHECK");
                        GLAPClient.instance.CompleteChthoniteCheck();
                        return false;
                    }
                    if (item == astralite)
                    {
                        GLAPModLoader.DebugShowMessage("ABOUT TO DO ASTRALITE CHEST CHECK");
                        GLAPClient.instance.CompleteAstraliteCheck();
                        return false;
                    }
                }
                else
                {

                    GLAPModLoader.DebugShowMessage("ITEM IN CHEST WAS NULL!!.");
                }

                //TODO: add chest checks here

            }
            else
            {

                GLAPModLoader.DebugShowMessage("NO LOOT ENTRIES!!!!");
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
