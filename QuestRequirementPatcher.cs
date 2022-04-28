using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GhostloreAP
{
    
    [HarmonyPatch(typeof(QuestRequirement), nameof(QuestRequirement.Passed))]
    public class QuestRequirementPatcher
    {
        static void Postfix(QuestInstance questInstance, QuestRequirement __instance, ref bool __result)
        {
            var ex = ExtendedBindingManager.instance.GetExtended<XQuestRequirement>(__instance);
            if(ex != null)
            {
                ex.StartListener();
                __result = ex.MetRequirement();
                if (__result==true)
                {
                    GLAPModLoader.DebugShowMessage("ABOUT TO FIRE EVENT FOR COMPLETE QUEST!!");
                    GLAPEvents.OnKillQuestCompleted?.Invoke(questInstance);
                }
            }
            
        }
    }
}
