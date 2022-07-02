using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostloreAP
{
    public static class GLAPEvents
    {
        public static Action<Creature,int> OnCreatureKilled;
        public static Action<XQuestRequirement> OnKillQuestCompleted;
    }
}
