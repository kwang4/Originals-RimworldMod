using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Originals
{
    public class Comp_Original : ThingComp
    {
        public bool checkOriginal = true; //If true, check if pawn can be original and add hediff if rand passes
        public bool isOriginal = false;
        public bool wasDead = false;
        public bool wasStaked = false;
        public int resTimer = 0;
        public int ticksTillHeal;
        public float stakeTickMult = 1f;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref checkOriginal, "O_checkOriginal", true);
            Scribe_Values.Look(ref isOriginal, "O_isOriginal", false);
            Scribe_Values.Look(ref wasDead, "O_wasDead", false);
            Scribe_Values.Look(ref wasStaked, "O_wasStaked", false);
            Scribe_Values.Look(ref resTimer, "O_resTimer", 0);
            Scribe_Values.Look(ref ticksTillHeal, "O_ticksTillHeal", 0);
            Scribe_Values.Look(ref stakeTickMult, "O_stakeTickMult", 1f);
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
                    float severity = 0.4f;
                    if (Rand.Chance(0.25f))
                        severity = .6f;
                    else if (Rand.Chance(0.45f))
                        severity = 1.0f;
                    else if(Rand.Chance(0.08f))
                    {
                        severity = 1.5f;
                    }
                    else if (Rand.Chance(0.01f))
                    {
                        severity = 2.5f;
                    }
                    addOriginalHediff(severity);
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
                RareLivingTick(pawn);
            }

        }

        public void TransferOriginalPower(Pawn pawn)
        {
            Map map = pawn.MapHeld;

            Pawn closest = null;
            int distance = 20;
            Hediff oHediff = pawn.health.hediffSet.GetFirstHediffOfDef(OriginalDefOf.Original, false);
            foreach (Pawn p in map.mapPawns.AllPawns)
            {
                int checkDist = GetDistance(p.Position, pawn.PositionHeld);
                if (p != pawn && !p.Dead && p.health.hediffSet.HasHediff(OriginalDefOf.Original, false) && checkDist < distance)
                {
                    closest = p;
                    distance = checkDist;
                }
            }
            if (closest != null)
            {
                

                Hediff oTransfer = closest.health.hediffSet.GetFirstHediffOfDef(OriginalDefOf.Original, false);
                oTransfer.Severity += oHediff.Severity * OriginalSettings.originalTransferPercent;
                map.weatherManager.eventHandler.AddEvent(new WeatherEvent_OriginalLightning(map, closest.Position, 0, true));
                map.weatherManager.eventHandler.AddEvent(new WeatherEvent_OriginalLightning(map, closest.Position, 0, true));
                GenExplosion.DoExplosion(pawn.PositionHeld, map, distance + 10f, DamageDefOf.EMP, null, -1, -1f, null, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false, null, null);
                Messages.Message("An Original has died and transferred their power",closest, MessageTypeDefOf.NeutralEvent, false);
            }
            else
            {
                GenExplosion.DoExplosion(pawn.PositionHeld, map, 35f, DamageDefOf.EMP, null, -1, -1f, null, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false, null, null);
                Messages.Message("An Original has died and released their power to the world", MessageTypeDefOf.PositiveEvent, false);
            }
            pawn.health.RemoveHediff(oHediff);


        }

        public void RareDeadTick(Pawn pawn)
        {

            Hediff oHediff = pawn.health.hediffSet.GetFirstHediffOfDef(OriginalDefOf.Original, false);
            Hediff oResHediff = pawn.health.hediffSet.GetFirstHediffOfDef(OriginalDefOf.O_ResStatus, false);
            Hediff stakedHediff = pawn.health.hediffSet.GetFirstHediffOfDef(OriginalDefOf.O_Staked, false);
            if (OriginalDefLoader.GetNotMissingPart(pawn, BodyPartDefOf.Heart) == null && OriginalSettings.needHeart) // remove immortality
            {
                isOriginal = false;
                if(oResHediff != null)
                {
                    pawn.health.RemoveHediff(oResHediff);
                }
                TransferOriginalPower(pawn);

            }

            if (wasDead == false)
            {
                wasDead = true;
                int firstTimeOffset = (oHediff.Severity < 0.5f) ? OriginalSettings.baseResTime : 0;
                setResTimer(Current.Game.tickManager.TicksGame + OriginalSettings.baseResTime + firstTimeOffset);
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

            if (stakedHediff != null && !wasStaked)
            {

                wasStaked = true;
                if (oHediff.Severity >= 0.5 && oResHediff != null)
                {
                    switch (OriginalStage(oHediff.Severity))
                    {
                        case 1:
                            stakeTickMult = OriginalSettings.lowStakeMult;
                            break;
                        case 2:
                            stakeTickMult = OriginalSettings.fullStakeMult;
                            break;
                        case 3:
                            stakeTickMult = OriginalSettings.highStakeMult;
                            break;
                        case 4:
                            stakeTickMult = OriginalSettings.originalStakeMult;
                            break;
                    }
                    Messages.Message(pawn.Name + " has been staked!", MessageTypeDefOf.NeutralEvent, false);
                }
                else if (oHediff.Severity < 0.5f)
                {
                    resTimer = Current.Game.tickManager.TicksGame + (int)(Math.Abs(resTimer - Current.Game.tickManager.TicksGame) * OriginalSettings.mortalStakeMult);
                }
            }

            if ((oResHediff != null && oResHediff.Severity == 0) || (oResHediff != null && oResHediff.Severity <= .13 && Rand.Chance(0.4f)) || (oHediff.Severity < 0.5f && Current.Game.tickManager.TicksGame >= resTimer) || (oResHediff != null && oResHediff.Severity == 2.0f))
            {
                if (oHediff.Severity < 0.5f)
                    oHediff.Severity = 0.5f; //Going from former mortal to lowblood

                ResPawn(pawn);
            }
        }

        public void RareLivingTick(Pawn pawn)
        {
            wasDead = false;
            wasStaked = false;
            stakeTickMult = 1f;
            if (Current.Game.tickManager.TicksGame >= ticksTillHeal)
            {
                Hediff oHediff = pawn.health.hediffSet.GetFirstHediffOfDef(OriginalDefOf.Original, false);

                if(oHediff != null && oHediff.Severity >= 0.5f)
                {
                    if(OriginalSettings.healScars)
                    {
                        TryHealScars(pawn);
                    }
                    TryHealWounds(pawn);
                    TryHealOldAge(pawn);
                }
                if (oHediff != null && oHediff.Severity >= 1f)
                {
                    TryRegenPart(pawn);
                }

                float multiplier = 1f;
                switch (pawn.health.hediffSet.GetFirstHediffOfDef(OriginalDefOf.Original, false).Severity)
                {
                    case float n when n < 1f:
                        multiplier = OriginalSettings.lowbloodMult; // Lowblood
                        break;
                    case float n when n < 1.5f:
                        multiplier = 1f;
                        break;
                    case float n when n < 2.5f:
                        multiplier = OriginalSettings.highbloodMult;
                        break;
                    case float n when n >= 2.5f:
                        multiplier = Math.Max(OriginalSettings.highbloodMult - .1f, 0); // Highblood
                        break;
                }
                ticksTillHeal = (int)(Current.Game.tickManager.TicksGame + OriginalSettings.ticksTillHeal * multiplier);
            }

        }

        public void TryHealOldAge(Pawn pawn)
        {
            
            IEnumerable<Hediff> hediffs = from hd in pawn.health.hediffSet.hediffs where OriginalDefLoader.oldAgeHediffs.Contains(hd.def) select hd;
            if (hediffs.Count() == 0)
            {
                return;
            }

            foreach(Hediff hediffToHeal in hediffs)
            {
                if (hediffToHeal != null)
                {
                    pawn.health.RemoveHediff(hediffToHeal);
                }
            }


        }

        public void TryHealScars(Pawn pawn)
        {
            IEnumerable<Hediff> hediffs = from hd in pawn.health.hediffSet.hediffs where hd.IsPermanent() && hd.def.isBad select hd;
            if (hediffs.Count() == 0)
                return;
            Hediff hediffToHeal = hediffs.RandomElement<Hediff>();
            if (hediffToHeal != null)
            {
                pawn.health.RemoveHediff(hediffToHeal);
            }

        }

        public void TryHealWounds(Pawn pawn)
        {
            IEnumerable<Hediff> hediffs = from hd in pawn.health.hediffSet.hediffs where hd.Bleeding || hd.IsTended() || hd.TendableNow(false) select hd;
            if (hediffs.Count() == 0)
                return;
            Hediff hediffToHeal = hediffs.RandomElement<Hediff>();
            if (hediffToHeal != null)
            {
                hediffToHeal.Severity = 0.2f;
                HediffWithComps hediffWithComps = hediffToHeal as HediffWithComps;
                if (hediffWithComps != null)
                {
                    HediffComp_TendDuration tend = hediffWithComps.TryGetComp<HediffComp_TendDuration>();
                    tend.tendQuality = 2f;
                    tend.tendTicksLeft = Find.TickManager.TicksGame;
                    pawn.health.Notify_HediffChanged(hediffToHeal);
                }


            }
        }

        public void TryRegenPart(Pawn pawn)
        {
            if (pawn.health.hediffSet.PainTotal >= 0.6)
            {
                return;
            }
            BodyPartRecord part = FindBiggestMissingBodyPartNotRegenerating(pawn, 0);
            if (part != null)
            {

                pawn.health.RestorePart(part, null, true);
                Hediff regrow = HediffMaker.MakeHediff(OriginalDefOf.O_RegenBodyPart, pawn, part);
                regrow.Severity = 1f * ((float)OriginalSettings.originalRegenPartTime / 60000f);
                if (part.coverageAbsWithChildren <= 0.1)
                    regrow.Severity *= 0.7f;
                pawn.health.AddHediff(regrow);
            }


        }

        private BodyPartRecord FindBiggestMissingBodyPartNotRegenerating(Pawn pawn, float minCoverage = 0f)
        {
            BodyPartRecord bodyPartRecord = null;
            foreach (Hediff_MissingPart hediff_MissingPart in pawn.health.hediffSet.GetMissingPartsCommonAncestors())
            {
                if (hediff_MissingPart.Part.coverageAbsWithChildren >= minCoverage && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(hediff_MissingPart.Part) && (bodyPartRecord == null || hediff_MissingPart.Part.coverageAbsWithChildren > bodyPartRecord.coverageAbsWithChildren))
                {
                    bodyPartRecord = hediff_MissingPart.Part;
                }
            }
            return bodyPartRecord;
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
                    
                    BodyPartRecord partParent = partDiff.Part;
                    while(partParent.parent != null)
                    {
                        //Log.Message(partDiff.Part + ", Needed: " + NeedPart(partDiff.Part));
                        if (partParent != null && NeedPart(partParent))
                        {
                            pawn.health.RestorePart(partParent, null, true);
                        }
                        else
                        {
                            regrowHediffParts.Add(partParent);
                        }
                        partParent = partParent.parent;
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
                
                if (hediff.def == OriginalDefOf.O_Staked || hediff.def == OriginalDefOf.HeartAttack)
                {
                   // Log.Message("Staked hediff: " + hediff.def.label);
                    pawn.health.RemoveHediff(hediff);
                    hediff.Severity = 0f;
                }
                if (hediff.TendableNow(false))
                {
                    hediff.Severity = 0.07f;
                    HediffWithComps h = hediff as HediffWithComps;
                    if (h != null)
                    {
                        
                        HediffComp_TendDuration tend = h.TryGetComp<HediffComp_TendDuration>();
                        if(tend != null)
                        {
                            tend.tendQuality = 0f;
                            tend.tendTicksLeft = Find.TickManager.TicksGame;
                        }
                        else
                        {
                            pawn.health.RemoveHediff(hediff);
                            hediff.Severity = 0f;
                        }


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
            Messages.Message(pawn.Name + " has resurrected!",pawn, MessageTypeDefOf.PositiveEvent, false);



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
            float missingPartOffset = hasMissingParts(3) ? (float)OriginalSettings.baseResTime / 2 : 0;
            oResHediff.Severity = Math.Max((float)(resTimer - Current.Game.tickManager.TicksGame + missingPartOffset) / (float)OriginalSettings.baseResTime * hediffStrengthMult * stakeTickMult, 0);

        }

        public float oHediffMultiplier(float severity)
        {
            float multiplier = 1f;
            switch (severity)
            {
                case float n when n < 1f:
                    multiplier = Math.Min(1 / severity, OriginalSettings.lowbloodMult); // Lowblood
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

        public int OriginalStage(float severity)
        {
            switch (severity)
            {
                case float n when n < 0.5f:
                    return 0; // Mortal
                case float n when n < 1f:
                    return 1; // Lowblood

                case float n when n < 1.5f:
                    return 2; // Fullblood

                case float n when n < 2.5f:
                    return 3; // Highblood

                case float n when n >= 2.5f:
                    return 4; // Highblood
            }
            return -1;
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

        public int GetDistance(IntVec3 vec1, IntVec3 vec2)
        {
            return Mathf.RoundToInt((float)Math.Sqrt(Math.Pow((double)Math.Abs(vec1.x - vec2.x), 2.0) + Math.Pow((double)Math.Abs(vec1.z - vec2.z), 2.0)));
        }

    }

}
