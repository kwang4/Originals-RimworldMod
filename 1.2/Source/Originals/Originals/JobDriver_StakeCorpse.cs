using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Originals
{
    class JobDriver_StakeCorpse : JobDriver
    {
        protected Corpse Victim
        {
            get
            {
                return (Corpse)this.job.targetA.Thing;
            }
        }
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(Victim, this.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden<JobDriver_StakeCorpse>(TargetIndex.A);
            this.FailOnAggroMentalState<JobDriver_StakeCorpse>(TargetIndex.A);
            Toil toil1 = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            yield return toil1;
            Toil toil2 = Toils_General.Wait(240, TargetIndex.None).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f).PlaySustainerOrSound(SoundDefOf.Interact_Tend);
            yield return toil2;

            yield return new Toil
            {
                initAction = () =>
                {
                    BodyPartRecord bodyPart = null;
                    foreach (BodyPartRecord part in Victim.InnerPawn.health.hediffSet.GetNotMissingParts())
                    {
                        foreach (BodyPartTagDef tag in part.def.tags)
                        {
                            if (tag.vital && tag.defName == "BloodPumpingSource")
                            {
                                bodyPart = part;
                                break;
                            }
                        }
                        if (bodyPart != null)
                            break;
                    }
                    while (!bodyPart.parent.IsCorePart)
                    {
                        bodyPart = bodyPart.parent;
                    }
                    if (bodyPart != null)
                    {
                        Hediff oHediff = Victim.InnerPawn.health.hediffSet.GetFirstHediffOfDef(OriginalDefOf.Original, false);
                        if (oHediff != null) //Victim is an Original
                        {
                            float severity = 1f;
                            switch (oHediff.Severity)
                            {
                                case float n when n < .5f:
                                    severity = OriginalSettings.mortalStakeMult; // Mortal
                                    break;
                                case float n when n < 1f:
                                    severity = OriginalSettings.lowStakeMult; // Lowblood
                                    break;
                                case float n when n < 1.5f:
                                    severity = OriginalSettings.fullStakeMult; // Fullblood
                                    break;
                                case float n when n < 2.5f:
                                    severity = OriginalSettings.highStakeMult; // Highblood
                                    break;
                                case float n when n >= 2.5f:
                                    severity = OriginalSettings.originalStakeMult; // Highblood
                                    break;
                            }
                            Hediff stakedHediff = HediffMaker.MakeHediff(OriginalDefOf.O_Staked, Victim.InnerPawn, OriginalDefLoader.GetNotMissingPart(Victim.InnerPawn, BodyPartDefOf.Heart));
                            stakedHediff.Severity = severity;
                            Victim.InnerPawn.health.AddHediff(stakedHediff);
                        }
                        else //Kill a normal pawn
                        {
                            DamageDef damageDef = DefDatabase<DamageDef>.GetNamed("Stab");
                            DamageInfo damage = new DamageInfo(damageDef, 1000, 999, -1f, pawn, bodyPart, null, DamageInfo.SourceCategory.ThingOrUnknown, null);

                            Hediff_Injury hediff = (Hediff_Injury)HediffMaker.MakeHediff(HediffDefOf.Stab, Victim.InnerPawn, bodyPart);
                            hediff.Severity = 1000f;
                            Victim.InnerPawn.health.AddHediff(hediff);
                        }

                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };



            yield break;
        }



    }
}
