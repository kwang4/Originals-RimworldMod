using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
namespace Originals
{
    public class OriginalSettings:ModSettings
    {
        public static int baseResTime = 70000;
        public static float originalChance = 0.05f;
        public static bool resSickness = false;
        public static bool healScars = true;
        public static float lowbloodMult = 1.3f;
        public static float fullbloodMult = .82f;
        public static float highbloodMult = 0.65f;
        public static float originalMult = .45f;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref baseResTime, "O_baseResTime", 60000);
            Scribe_Values.Look(ref originalChance, "O_originalChance", 0.05f);
            Scribe_Values.Look(ref resSickness, "O_resSickness", false);
            Scribe_Values.Look(ref healScars, "O_healScars", true);
            Scribe_Values.Look(ref lowbloodMult, "O_lowbloodMult", 1.3f);
            Scribe_Values.Look(ref fullbloodMult, "O_fullbloodMult", 0.85f);
            Scribe_Values.Look(ref highbloodMult, "O_highbloodMult", 0.7f);
            Scribe_Values.Look(ref originalMult, "O_originalMult", 0.45f);
            base.ExposeData();

        }

    }
}
