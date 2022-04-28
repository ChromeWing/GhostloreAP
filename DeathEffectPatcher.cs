using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GhostloreAP
{
    [HarmonyPatch(typeof(DeathEffect),nameof(DeathEffect.TriggerDeath))]
    public class DeathEffectPatcher
    {
        static void Prefix(CharacterContainer defender)
        {
            if (defender.IsDead) { return; }
            GLAPEvents.OnCreatureKilled?.Invoke(defender.Creature);
        }
    }
}
