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
    class JobDriver_StabilizeHere : JobDriver
    {

        private const TargetIndex targetInd = TargetIndex.A;

        protected Pawn Patient
        {
            get
            {
                return (Pawn)job.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected Pawn Doctor
        {
            get
            {
                return this.pawn;
            }
        }

        protected Thing MedicineUsed
        {
            get
            {
                return this.job.targetB.Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return Doctor.Reserve(Patient, this.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden<JobDriver_StabilizeHere>(TargetIndex.A);
            this.FailOnAggroMentalState<JobDriver_StabilizeHere>(TargetIndex.A);
            this.AddEndCondition(() => {
                if (HealthAIUtility.ShouldBeTendedNowByPlayer(this.Patient) || this.Patient.health.HasHediffsNeedingTend(false))
                {
                    return JobCondition.Ongoing;
                }
                return JobCondition.Succeeded;
            });

            Toil toil1 = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return toil1;

            Toil toil2 = Toils_General.Wait((int)(1f / this.Doctor.GetStatValue(StatDefOf.MedicalTendSpeed, true) * 600f), TargetIndex.None).FailOnCannotTouch<Toil>(TargetIndex.A, PathEndMode.InteractionCell).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f).PlaySustainerOrSound(SoundDefOf.Interact_Tend);
            toil2.activeSkill = () => SkillDefOf.Medicine;
            yield return toil2;

            yield return Toils_Tend.FinalizeTend(this.Patient);

            yield return Toils_Jump.Jump(toil1);
        }
    }
}
