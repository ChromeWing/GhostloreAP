using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GhostloreAP
{
    [HarmonyPatch(typeof(Chest),nameof(GoalInteractableObject.OnReachedGoal))]
    public class LootSpawnPatcher
    {
        static Item chthonite, astralite;

        static bool Prefix(LootTable ___lootTable, ref bool ___opened)
        {
            GLAPModLoader.DebugShowMessage("GOING TO OPEN A CHEST.");
            if (___opened) 
            {
                GLAPModLoader.DebugShowMessage("Already opened????");
                return true; 
            }
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
                        ___opened = true;
                        return false;
                    }
                    if (item == astralite)
                    {
                        GLAPModLoader.DebugShowMessage("ABOUT TO DO ASTRALITE CHEST CHECK");
                        GLAPClient.instance.CompleteAstraliteCheck();
                        ___opened = true;
                        return false;
                    }
                }
                else
                {

                    GLAPModLoader.DebugShowMessage("ITEM IN CHEST WAS NULL!!.");
                }

                //TODO: add chest checks here
                //it's a normal chest, do a chest location check.
                GLAPModLoader.DebugShowMessage("Going to do a chest check!!");
                if (GLAPClient.instance.CompleteChestCheck())
                {
                    ___opened = true;
                    return false;
                }
                else
                {
                    return true;
                }
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
