using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
namespace Originals
{
    public class Comp_Original : ThingComp
    {
        public bool checkOriginal = true; //If true, check if pawn can be original and add hediff if rand passes
        public bool isOriginal = false;
        public bool wasDead = false;
        public int resTimer = 0;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref checkOriginal, "checkOriginal", true);
            Scribe_Values.Look(ref isOriginal, "isOriginal", false);
            Scribe_Values.Look(ref wasDead, "wasDead", false);
            Scribe_Values.Look(ref resTimer, "resTimer", 0);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (respawningAfterLoad)
            {
                checkOriginal = false;
            }
            if (checkOriginal)
            {
                isOriginal = CanBeOriginal() && Rand.Chance(0.05f);
                Pawn p = parent as Pawn;
                if (p.health.hediffSet.HasHediff(OriginalDefOf.Original))
                {
                    isOriginal = true;
                }
                else if(isOriginal)
                {
                    addOriginalHediff(0.3f);
                }

                //Remove possibly unwanted originals
                if(!CanBeOriginal())
                {
                    removeOriginalHediff();
                }

            }

        }




        public override void CompTickRare()
        {
            base.CompTickRare();
            Pawn pawn = parent as Pawn;
            if (pawn.health.hediffSet.HasHediff(OriginalDefOf.Original, false)) //Check if hediff magically dissapears (devmode)
                isOriginal = true;
            else
            {
                isOriginal = false;
                return;
            }
                



            if(pawn.Dead)
            {
                Hediff oHediff = pawn.health.hediffSet.GetFirstHediffOfDef(OriginalDefOf.Original, false);
                Hediff oresHediff = pawn.health.hediffSet.GetFirstHediffOfDef(OriginalDefOf.O_ResTimer, false);
                if (wasDead == false)
                {
                    wasDead = true;
                    setResTimer(Current.Game.tickManager.TicksGame + OriginalSettings.baseResTime);
                }
                if(oHediff.Severity >= 0.3f) //Timer hediff
                {
                    //Add timer hediff
                }
                Log.Message(Current.Game.tickManager.TicksGame.ToString());
                if (Current.Game.tickManager.TicksGame >= resTimer)
                {
                    ResurrectionUtility.Resurrect(pawn);
                    pawn.health.Notify_Resurrected();
                }
            }
            else
            {
                wasDead = false;
            }

        }

        public int getResurrectionLength()
        {

            return 0;
        }

        public bool CanBeOriginal()
        {
            Pawn pawn = parent as Pawn;
            if (pawn.RaceProps.FleshType == FleshTypeDefOf.Insectoid || pawn.RaceProps.FleshType == FleshTypeDefOf.Mechanoid
                || pawn.RaceProps.FleshType.defName.Contains("Android") || pawn.RaceProps.FleshType.defName.Contains("Artificial"))
            {
                return false;
            }
            return true;
        }

        public void addOriginalHediff(float severity)
        {
            Pawn p = parent as Pawn;
            Hediff originalHediff = HediffMaker.MakeHediff(OriginalDefOf.Original, p, null);
            originalHediff.Severity = severity;
            p.health.AddHediff(originalHediff);
        }

        public void removeOriginalHediff()
        {
            Pawn p = parent as Pawn;
            Hediff originalHediff = p.health.hediffSet.GetFirstHediffOfDef(OriginalDefOf.Original, false);
            if (originalHediff != null)
                
                p.health.RemoveHediff(originalHediff);
        }


        public void removeTimerHediff()
        {
            Pawn p = parent as Pawn;
            Hediff timerHediff = p.health.hediffSet.GetFirstHediffOfDef(OriginalDefOf.Original, false);
            if (timerHediff != null)
                p.health.RemoveHediff(timerHediff);
        }

        public void setResTimer(int ticks)
        {
            resTimer = ticks;
        }

    }

}
