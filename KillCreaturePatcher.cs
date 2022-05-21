using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine.Events;

namespace GhostloreAP
{
    [HarmonyPatch(typeof(ResourceContainer),nameof(ResourceContainer.KillCreature))]
    public class KillCreaturePatcher
    {
        static bool Prefix(CharacterContainer killer,UnityEvent<CharacterContainer,CharacterContainer> ___onDeath,CharacterContainer ___character)
        {
            ___onDeath.Invoke(killer, ___character);
            //never drop loot from a killed creature in archipelago. (loot drops come from Items granted from the multiworld instead.)
            if (___character.Creature == null) { return false; }
            return QuestFactory.instance.CompletedAllKillsForCreature(___character.Creature);
        }
    }
}
