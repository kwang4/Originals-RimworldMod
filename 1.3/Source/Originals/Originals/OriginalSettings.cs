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
        public static bool resSickness = false;
        public static float originalChance = 0.05f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref baseResTime, "baseResTime", 60000);
            Scribe_Values.Look(ref resSickness, "resSickness", false);
            Scribe_Values.Look(ref originalChance, "originalChance", 0.05f);
            base.ExposeData();

        }

    }
}
