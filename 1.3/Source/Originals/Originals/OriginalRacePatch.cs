using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
namespace Originals
{
    [StaticConstructorOnStartup]
    public static class OriginalRacePatch
    {
        static OriginalRacePatch()
        {
            PatchDefs();
        }
        public static void PatchDefs()
        {
            foreach(ThingDef thing in DefDatabase<ThingDef>.AllDefs)
            {
                if(thing?.race?.Humanlike ?? false)
                {
                    if (thing.comps == null)
                    {
                        thing.comps = new List<CompProperties>();
                    }
                        
                    thing.comps.Add(new CompProperties_Original());
                   // Log.Message($"{thing.defName} patched by Originals");
                }
            }
        }
    }
}
