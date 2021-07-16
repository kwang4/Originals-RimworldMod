using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimWorld.Planet;

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
                isOriginal = CanBeOriginal() && Rand.Chance(OriginalSettings.originalChance);
                Pawn p = parent as Pawn;
                if (p.health.hediffSet.HasHediff(OriginalDefOf.Original))
                {
                    isOriginal = true;
                }
                else if (isOriginal)
                {
                    addOriginalHediff(0.3f);
                }

                //Remove possibly unwanted originals
                if (!CanBeOriginal())
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


            Log.Message("Ticking");

            if (pawn.Dead)
            {
                RareDeadTick(pawn);
            }
            else
            {
                RareLivingTick();
            }

        }

        public void RareDeadTick(Pawn pawn)
        {

            Hediff oHediff = pawn.health.hediffSet.GetFirstHediffOfDef(OriginalDefOf.Original, false);
            Hediff oResHediff = pawn.health.hediffSet.GetFirstHediffOfDef(OriginalDefOf.O_ResStatus, false);
            if (wasDead == false)
            {
                wasDead = true;
                setResTimer(Current.Game.tickManager.TicksGame + OriginalSettings.baseResTime);
            }
            if (oHediff.Severity >= 0.5f) //Timer hediff
            {
                if (oResHediff == null)
                {
                    oResHediff = HediffMaker.MakeHediff(OriginalDefOf.O_ResStatus, pawn, null);
                    pawn.health.AddHediff(oResHediff);
                }

                setResHediffSeverity(oResHediff);

            }
            if (oResHediff.Severity == 0 || (oResHediff.Severity <= .13 && Rand.Chance(0.4f)))
            {
                if (oHediff.Severity < 0.5f)
                    oHediff.Severity = 0.5f; //Going from former mortal to lowblood

                ResPawn(pawn);
            }
        }

        public void RareLivingTick()
        {
            wasDead = false;
        }

        public void ResPawn(Pawn pawn)
        {
            int tile = -1;
            Caravan pawnCaravan = null;
            tile = pawn?.Corpse?.Tile ?? -1;
            
            if (tile != -1)
            {
                //Caravan Checks
                List<Caravan> caravans = new List<Caravan>();
                Find.World.worldObjects.GetPlayerControlledCaravansAt(tile, caravans);
                foreach (Caravan c in caravans)
                {
                    if (c.AllThings.Contains(pawn.Corpse))
                    {
                        pawnCaravan = c;

                        break;
                    }
                }
            }
            if(!pawn.Corpse.Spawned && pawnCaravan == null)
            {
                GenSpawn.Spawn(pawn.Corpse, pawn.PositionHeld, pawn.MapHeld, WipeMode.Vanish);
            }
            Thing storage = pawn.Corpse.StoringThing();
            if (storage != null && pawnCaravan == null)
            {
                GenSpawn.Spawn(pawn.Corpse, storage.Position, storage.Map, WipeMode.Vanish);

            }
            ResurrectionUtility.Resurrect(pawn);
            pawn.health.Notify_Resurrected();
            removeResStatusHediff();
            if (pawnCaravan != null)
                pawnCaravan.AddPawn(pawn, false);
            else
                GenExplosion.DoExplosion(pawn.Position, pawn.Map, 9f, DamageDefOf.Smoke, pawn);

            Messages.Message(pawn.Name + " has resurrected!", MessageTypeDefOf.PositiveEvent, false);

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


        public void removeResStatusHediff()
        {
            Pawn p = parent as Pawn;
            Hediff oResHediff = p.health.hediffSet.GetFirstHediffOfDef(OriginalDefOf.O_ResStatus, false);
            if (oResHediff != null)
                p.health.RemoveHediff(oResHediff);
        }

        public bool hasMissingParts()
        {
            return hasMissingParts(1);
        }
        public bool hasMissingParts(int breakOnNum)
        {
            int counter = 0;
            foreach (Hediff hediff in ((IEnumerable<Hediff>)(parent as Pawn).health.hediffSet.hediffs.ToList<Hediff>()))
            {
                if (hediff.GetType() == typeof(Hediff_MissingPart))
                {
                    counter++;
                }
                if (counter >= breakOnNum)
                    return true;
            }
            return false;
        }

        public void setResHediffSeverity(Hediff oResHediff)
        {
            float missingPartOffset = hasMissingParts(2) ? (float)OriginalSettings.baseResTime / 3 : 0;
            oResHediff.Severity = Math.Max((float)(resTimer - Current.Game.tickManager.TicksGame + missingPartOffset) / (float)OriginalSettings.baseResTime, 0);

        }

        public void setResTimer(int ticks)
        {
            resTimer = ticks;
        }

    }

}
