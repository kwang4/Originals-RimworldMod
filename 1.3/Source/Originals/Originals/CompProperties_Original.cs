using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
namespace Originals
{
    public class CompProperties_Original:CompProperties
    {
        public CompProperties_Original()
        {
            this.compClass = typeof(Comp_Original);
        }
    }
}
