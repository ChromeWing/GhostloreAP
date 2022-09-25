using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace GhostloreAP
{
    [HarmonyPatch(typeof(InventoryPanel),
        nameof(InventoryPanel.ReceiveDraggedItem),
        new[] { 
            typeof(Vector2Int), 
            typeof(ItemInstance), 
            typeof(InventoryPanel), 
            typeof(bool),
            typeof(ItemContextAction)
        }, 
        new[] { 
            ArgumentType.Normal,
            ArgumentType.Normal, 
            ArgumentType.Normal, 
            ArgumentType.Normal, 
            ArgumentType.Ref 
    })]
    public class InventoryPanelPatcher
    {
        static void Postfix(ItemInstance itemInstance, CharacterContainer ___player, ref ItemContextAction contextAction, Inventory ___inventoryTarget)
        {
            bool goingToYourInventory = false;
            foreach(var i in ___player.GetCharacterComponents<Inventory>())
            {
                if (i == null) { continue; }
                if (i.ContainsItem(itemInstance))
                {
                    goingToYourInventory = true;
                    break;
                }
            }

            if (contextAction == ItemContextAction.Buy && goingToYourInventory)
            {
                GLAPModLoader.DebugShowMessage("We are in the buying...");
                XItemInstance ex = ExtendedBindingManager.instance.GetExtended<XItemInstance>(itemInstance);
                if(ex == null) { return; }

                GLAPLocationManager.instance.CompleteShopCheck(ex.AP_ShopSlot);

                ___inventoryTarget.RemoveItem(itemInstance);

            }
        }
    }
}
