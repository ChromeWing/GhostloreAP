﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TMPro;

namespace GhostloreAP
{
    [HarmonyPatch(typeof(ItemToolTipFormat),nameof(ItemToolTipFormat.BindTooltip))]
    public class ItemToolTipFormatPatcher
    {
        static void Postfix(Item item, ItemInstance itemInstance, ItemToolTipFormat __instance)
        {
            if(item == null || itemInstance == null) { return; }
            XItemInstance xItem = ExtendedBindingManager.instance.GetExtended<XItemInstance>(itemInstance);
            if (xItem != null)
            {
                Traverse.Create(__instance).Field("title").GetValue<TextMeshProUGUI>().text = xItem.overrideItem.ItemName;
                Traverse.Create(__instance).Field("description").GetValue<TextMeshProUGUI>().text = xItem.overrideItem.Description;
                Traverse.Create(__instance).Field("description").GetValue<TextMeshProUGUI>().gameObject.SetActive(true);
                Traverse.Create(__instance).Field("tags").GetValue<TextMeshProUGUI>().text = "Archipelago Shop Item";
                Traverse.Create(__instance).Field("coreMods").GetValue<TextMeshProUGUI>().gameObject.SetActive(false);
                Traverse.Create(__instance).Field("mods").GetValue<TextMeshProUGUI>().gameObject.SetActive(false);

            }

        }

    }
}
