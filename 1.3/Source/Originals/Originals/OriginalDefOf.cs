using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Originals
{
 [DefOf]
public static class OriginalDefOf
    {
        static OriginalDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(OriginalDefOf));
        }
        public static HediffDef Original;
        public static HediffDef O_ResStatus;
    }
}
