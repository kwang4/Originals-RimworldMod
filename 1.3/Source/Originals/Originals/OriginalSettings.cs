using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
namespace Originals
{
    public class OriginalSettings:ModSettings
    {
 
        public static int baseResTime = 50000;
        public static float originalChance = 0.05f;
        public static bool needHeart = true;
        public static bool resSickness = false;
        public static bool healScars = true;
        public static float lowbloodMult = 1.3f;
        public static float fullbloodMult = .82f;
        public static float highbloodMult = 0.65f;
        public static float originalMult = .35f;
        public static float originalTransferPercent = 0.2f;
        public static int originalRegenPartTime = 50000;
        public static int ticksTillHeal = 1800;
        public static float mortalStakeMult = 4f;
        public static float lowStakeMult = 3.5f;
        public static float fullStakeMult = 2.5f;
        public static float highStakeMult = 1.8f;
        public static float originalStakeMult = 1.3f;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref baseResTime, "O_baseResTime", 50000);
            Scribe_Values.Look(ref originalChance, "O_originalChance", 0.05f);
            Scribe_Values.Look(ref needHeart, "O_needHeart", true);
            Scribe_Values.Look(ref resSickness, "O_resSickness", false);
            Scribe_Values.Look(ref healScars, "O_healScars", true);
            Scribe_Values.Look(ref lowbloodMult, "O_lowbloodMult", 1.3f);
            Scribe_Values.Look(ref fullbloodMult, "O_fullbloodMult", 0.85f);
            Scribe_Values.Look(ref highbloodMult, "O_highbloodMult", 0.65f);
            Scribe_Values.Look(ref originalMult, "O_originalMult", 0.35f);
            Scribe_Values.Look(ref originalTransferPercent, "O_originalTransferPercent", 0.2f);
            Scribe_Values.Look(ref originalRegenPartTime, "O_originalRegenPartTime", 50000);
            Scribe_Values.Look(ref ticksTillHeal, "O_ticksTillHeal", 1800);
            Scribe_Values.Look(ref mortalStakeMult, "O_mortalStakeMult", 4f);
            Scribe_Values.Look(ref lowStakeMult, "O_lowStakeMult", 3.5f);
            Scribe_Values.Look(ref fullStakeMult, "O_fullStakeMult", 2.5f);
            Scribe_Values.Look(ref highStakeMult, "O_highStakeMult", 1.8f);
            Scribe_Values.Look(ref originalStakeMult, "O_originalStakeMult", 1.3f);
            base.ExposeData();


        }



    }
}
