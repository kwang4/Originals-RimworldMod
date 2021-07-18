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
                int firstTimeOffset = (oHediff.Severity < 0.5f) ? OriginalSettings.baseResTime : 0;
                setResTimer(Current.Game.tickManager.TicksGame + OriginalSettings.baseResTime+ firstTimeOffset);
            }
            if (oHediff.Severity >= 0.5f) //Timer hediff
            {
                if (oResHediff == null)
                {
                    oResHediff = HediffMaker.MakeHediff(OriginalDefOf.O_ResStatus, pawn, null);
                    pawn.health.AddHediff(oResHediff);
                }

                setResHediffSeverity(oResHediff, oHediff);

            }
            if ((oResHediff != null && oResHediff.Severity == 0) || (oResHediff != null && oResHediff.Severity <= .13 && Rand.Chance(0.4f)) || (oHediff.Severity < 0.5f && Current.Game.tickManager.TicksGame >= resTimer) || (oResHediff.Severity == 2.0f))
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
            if (pawn == null || pawn.Corpse == null)
                return;
            #region hediffs


            List<BodyPartRecord> regrowHediffParts = new List<BodyPartRecord>();
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs.ToList())
            {
                if (hediff.GetType() == typeof(Hediff_MissingPart))
                {
                    Hediff_MissingPart partDiff = hediff as Hediff_MissingPart;
                    if (partDiff.Part != null && NeedPart(partDiff.Part))
                    {
                        pawn.health.RestorePart(partDiff.Part,null,true);
                    }
                    else
                    {
                        regrowHediffParts.Add(partDiff.Part);
                    }
                }
            }

            Pawn_HealthTracker health = pawn.health;
            HediffSet hediffSet = health.hediffSet;
            ImmunityHandler immunity = health.immunity;
            pawn.health.hediffSet = new HediffSet(pawn);
            pawn.health.immunity = new ImmunityHandler(pawn);
            


            #endregion

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
            if (!pawn.Corpse.Spawned && pawnCaravan == null)
            {
                GenSpawn.Spawn(pawn.Corpse, pawn.PositionHeld, pawn.MapHeld, WipeMode.Vanish);
            }
            Thing storage = pawn.Corpse.StoringThing();
            if (storage != null && pawnCaravan == null)
            {
                GenSpawn.Spawn(pawn.Corpse, storage.Position, storage.Map, WipeMode.Vanish);

            }
            if (OriginalSettings.resSickness)
                ResurrectionUtility.ResurrectWithSideEffects(pawn);
            else
                ResurrectionUtility.Resurrect(pawn);

            foreach (Hediff hediff in hediffSet.hediffs.ToList())
            {
                if (hediff.TendableNow(false))
                {
                    hediff.Severity = 0.07f;
                    HediffWithComps h = hediff as HediffWithComps;
                    if (h != null)
                    {
                        HediffComp_TendDuration tend = h.TryGetComp<HediffComp_TendDuration>();
                        tend.tendQuality = 0f;
                        tend.tendTicksLeft = Find.TickManager.TicksGame;

                    }
                }
                if (hediff.GetType() == typeof(Hediff_MissingPart))
                {
                    (hediff as Hediff_MissingPart).IsFresh = false;
                }
            }
            DamageInfo dmg = new DamageInfo(DamageDefOf.Blunt, 0.5f, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null);
            pawn.health.hediffSet = hediffSet;
           // pawn.TakeDamage(dmg);
            pawn.health.immunity = immunity;
            removeResStatusHediff();
            if (pawnCaravan != null)
                pawnCaravan.AddPawn(pawn, false);
            else
                GenExplosion.DoExplosion(pawn.Position, pawn.Map, 9f, DamageDefOf.Smoke, pawn);

            Messages.Message(pawn.Name + " has resurrected!", MessageTypeDefOf.PositiveEvent, false);



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

        public void setResHediffSeverity(Hediff oResHediff, Hediff oHediff)
        {
            float hediffStrengthMult = oHediffMultiplier(oHediff.Severity);
            float missingPartOffset = hasMissingParts(2) ? (float)OriginalSettings.baseResTime / 2 : 0;
            oResHediff.Severity = Math.Max((float)(resTimer - Current.Game.tickManager.TicksGame + missingPartOffset) / (float)OriginalSettings.baseResTime * hediffStrengthMult, 0);

        }

        public float oHediffMultiplier(float severity)
        {
            float multiplier = 1f;
            switch(severity)
            {
                case float n when n < 1f:
                    multiplier = Math.Min(1/severity,OriginalSettings.lowbloodMult); // Lowblood
                    break;
                case float n when n < 1.5f:
                    multiplier = Math.Min(1 / severity, OriginalSettings.fullbloodMult); // Fullblood
                    break;
                case float n when n < 2.5f:
                    multiplier = Math.Min(1 / severity, OriginalSettings.highbloodMult); // Highblood
                    break;
                case float n when n >= 2.5f:
                    multiplier = Math.Min(1 / severity, OriginalSettings.originalMult); // Highblood
                    break;
            }
            return multiplier;
        }

        public void setResTimer(int ticks)
        {
            resTimer = ticks;
        }
        public bool NeedPart(BodyPartRecord part)
        {
            if (part != null)
            {
                using (List<BodyPartTagDef>.Enumerator enumerator2 = part.def.tags.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        if (enumerator2.Current.vital)
                        {
                            return true;
                        }
                    }

                }
                return false;
            }
            return false;
        }

    }

}
