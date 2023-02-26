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
        static void Prefix(CharacterContainer attacker,CharacterContainer defender)
        {
            if (defender.IsDead) { return; }
            if (defender == PlayerManager.instance.GetFirstPlayer()) { 
                //this is us.
                
                return; 
            }

            int killCount_ = 1;
            if((defender.State & CharacterContainerState.Champion) != CharacterContainerState.None)
            {
                killCount_ = 3;
            }else if ((defender.State & CharacterContainerState.Elite) != CharacterContainerState.None)
            {
                killCount_ = 5;
            }
            GLAPEvents.OnCreatureKilled?.Invoke(defender,killCount_);
        }
    }
}
