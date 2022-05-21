using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostloreAP
{

    public enum MonsterWorkload
    {
        TEST,
        QuickPlaythrough,
        SinglePlaythrough,
        SomeGrinding,
        GrindingRequired
    }

    public enum ItemLevelType
    {
        TiedToCharacterLevel,
        TiedToProgression
    }

    public static class GLAPSettings
    {
        public static ItemLevelType itemLevelType = ItemLevelType.TiedToCharacterLevel;
        public static MonsterWorkload workload = MonsterWorkload.TEST;
    }
}
