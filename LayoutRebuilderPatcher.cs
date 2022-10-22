using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Pool;

namespace GhostloreAP
{
    //this is one of the only patches I think I ever had to make to something Unity engine related...
    [HarmonyPatch(typeof(LayoutRebuilder), "PerformLayoutCalculation")]
    public class LayoutRebuilderPatcher
    {
        static bool Prefix(RectTransform rect)
        {
            List<Component> list = CollectionPool<List<Component>, Component>.Get();
            rect.GetComponents(typeof(ILayoutElement), list);
            if(list == null)
            {
                //list is somehow null, don't allow LayoutRebuilder.StripDisabledBehavioursFromList() to get called and break.
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(LayoutRebuilder), "PerformLayoutControl")]
    public class LayoutRebuilderPatcher2
    {
        static bool Prefix(RectTransform rect)
        {
            List<Component> list = CollectionPool<List<Component>, Component>.Get();
            rect.GetComponents(typeof(ILayoutController), list);
            if (list == null)
            {
                //list is somehow null, don't allow LayoutRebuilder.StripDisabledBehavioursFromList() to get called and break.
                return false;
            }
            return true;
        }
    }
}
