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

    //cleanup the XItemInstances if they are wiped clean from the shop!
    [HarmonyPatch(typeof(ItemManager),nameof(ItemManager.DestroyInventory))]
    public class DestroyInventoryPatcher
    {
        static bool Prefix(int id, Dictionary<int,InventoryData> ___itemsInInventory)
        {
            bool foundBracelets = false;
            if (___itemsInInventory.TryGetValue(id,out InventoryData data_))
            { 
                foreach(ItemInstance item in data_.Items)
                {
                    var xItem = ExtendedBindingManager.instance.GetExtended<XItemInstance>(item);
                    if (xItem!=null)
                    {
                        foundBracelets = true;
                        break;
                    }
                }
            }
            if (foundBracelets)
            {
                ExtendedBindingManager.instance.EraseBindingsOfType<XItemInstance>();
            }
            return true;
        }
    }
}
