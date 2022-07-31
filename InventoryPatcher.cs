using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GhostloreAP
{
    [HarmonyPatch(typeof(Inventory),nameof(Inventory.CanAddItem))]
    public class InventoryPatcher
    {
        static Item chthonite;
        static Item astralite;

        static void InitQuestItems()
        {
            chthonite = ItemManager.instance.GetItemFromName("Chthonite");
            astralite = ItemManager.instance.GetItemFromName("Astralite");
        }
        static void Postfix(ItemInstance item,ref bool __result,Inventory __instance)
        {
            //GLAPModLoader.DebugShowMessage("CANADDITEM: item="+item.Item.NameDisplay+" result="+__result+", item contains="+ (__instance.Items.Where(i => i.Item == item.Item).Count() > 0 )+ ", IsAPQuestItem="+IsAPQuestItem(item));
            if(__result && __instance.Items.Where(i=>i.Item==item.Item).Count()>0 && IsAPQuestItem(item))
            {
                GLAPNotification.instance.DisplayLog("You already have that quest item.");
                __result = false;
            }
        }

        static bool IsAPQuestItem(ItemInstance item)
        {
            if(chthonite==null || astralite == null) { InitQuestItems(); }
            return item.Item == chthonite || item.Item == astralite;
        }
    }
}
