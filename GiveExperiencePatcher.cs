using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GhostloreAP
{
    [HarmonyPatch(typeof(GiveExperience), "CalculatedExp")]
    public class GiveExperiencePatcher
    {
        static void Postfix(ref float __result)
        {
            __result = __result * GLAPSettings.ExperienceMultiplier;
        }
    }
}
