using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GhostloreAP
{
    [HarmonyPatch(typeof(PlayerDeath),nameof(PlayerDeath.OnDeath))]
    public class PlayerDeathPatcher
    {
        static bool Prefix(CharacterContainer attacker,CharacterContainer defender, ref float ___resurrectTimer)
        {
            if (!defender.IsDead && attacker!=null && GLAPSettings.deathlink)
            {
                GLAPModLoader.DebugShowMessage("Going to die.");
                GLAPClient.instance.ReportDeath(attacker);
            }
            return true;
        }
    }
}
