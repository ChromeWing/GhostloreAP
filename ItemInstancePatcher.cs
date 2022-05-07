using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace GhostloreAP
{
    [HarmonyPatch(typeof(ItemInstance),nameof(ItemInstance.SetSprite))]
    public class ItemInstancePatcher
    {
        static void Postfix(Image[] images, SetSpriteSettings settings, ItemInstance __instance)
        {
            XItemInstance ex = ExtendedBindingManager.instance.GetExtended<XItemInstance>(__instance);
            if (ex != null)
            {
                Helpers.SetSprite(images[0], ItemFactory.instance.Bracelet, 0f, false, .5f, true);
            }
        }
    }
}
