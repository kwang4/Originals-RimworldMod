using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
namespace Originals
{
    [StaticConstructorOnStartup]
    class OriginalDefLoader
    {
        public static List<HediffDef> oldAgeHediffs = new List<HediffDef>();
         static OriginalDefLoader()
        {
            loadAgeDefs();
        }
        public static void loadAgeDefs()
        {
            if (oldAgeHediffs.Count == 0)
            {
                foreach (HediffGiverSetDef hediffGiverSet in DefDatabase<HediffGiverSetDef>.AllDefs)
                {
                    foreach (HediffGiver hediffGiver in hediffGiverSet.hediffGivers)
                    {
                        if (hediffGiver.ToString().ToLower().Contains("birthday") && hediffGiver.hediff.isBad && !oldAgeHediffs.Contains(hediffGiver.hediff))
                        {
                            oldAgeHediffs.Add(hediffGiver.hediff);
                        }
                    }
                }
            }
        }
    }
}
