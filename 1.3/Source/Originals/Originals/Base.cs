using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Originals
{
    public class Base
    {
    }
    public static class Patches
    {
        [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
        public static class FloatMenuMakerCarryAdder
        {
            [HarmonyPostfix]
            public static void AddHumanlikeOrdersPostfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
            {

                if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || pawn.WorkTagIsDisabled(WorkTags.Caring) || pawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor) || !pawn.workSettings.WorkIsActive(WorkTypeDefOf.Doctor))
                {
                    return;
                }

                foreach (LocalTargetInfo localTargetInfo in GenUI.TargetsAt_NewTemp(clickPos, TargetingParameters.ForRescue(pawn), true))
                {

                    Pawn target = (Pawn)localTargetInfo.Thing;

                    if (!target.health.HasHediffsNeedingTend())
                    {
                        continue;
                    }

                    if (!pawn.CanReserveAndReach(target, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, true))
                    {
                        // TODO: Add grayed out message
                        continue;
                    }

                    JobDef stabilizeJD = JobDefOf.AttackMelee;

                    Action action = () => {
                        Job job = new Job(stabilizeJD, target);
                        job.count = 1;

                        pawn.jobs.TryTakeOrderedJob(job);
                    };

                    string text = "Jack off Pawn".Translate(target.LabelCap, target);
                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, action, MenuOptionPriority.AttackEnemy, null, target, 0f, null, null), pawn, target, "ReservedBy"));
                }

            }
        }
    }

}
