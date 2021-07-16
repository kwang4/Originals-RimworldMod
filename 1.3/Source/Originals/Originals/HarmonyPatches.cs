using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Originals
{
   [StaticConstructorOnStartup]
   public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("Kwang4.Originals");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(Corpse), "ShouldVanish", MethodType.Getter)]
    public static class Patch_Corpse_ShouldVanish
    {
        [HarmonyPrefix]
        public static bool Prefix(Corpse __instance, ref bool __result)
        {
            if (__instance == null || __instance.InnerPawn == null)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    public static class FloatMenuMakerCarryAdder
    {
        [HarmonyPostfix]
        public static void Postfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                return;
            IntVec3 c = IntVec3.FromVector3(clickPos);
            foreach (Thing t in c.GetThingList(pawn.Map))
            {
                Pawn target = t as Pawn;
                if (target != null)
                {
                    if (!pawn.CanReserveAndReach(target, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, true))
                    {
                        continue;
                    }
                    JobDef killTarget = DefDatabase<JobDef>.GetNamed("StakePawn");
                    Action action = () =>
                    {
                        Job job = new Job(killTarget, target);
                        job.count = 1;
                        pawn.jobs.TryTakeOrderedJob(job);
                    };
                    string text = "Stake pawn";
                    FloatMenuOption menuOption = new FloatMenuOption(text, action, MenuOptionPriority.AttackEnemy, null, null, 0f, null, null);
                    opts.Add(menuOption);
                }
            }
            foreach (LocalTargetInfo targetInfo in GenUI.TargetsAt(clickPos, TargetingParameters.ForAttackAny(), true, null))
            {
                Pawn target = (Pawn)targetInfo.Thing;
                if (!target.Downed && target.Awake())
                    continue;
                if (!pawn.CanReserveAndReach(target, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, true))
                {
                    continue;
                }
                JobDef killTarget = DefDatabase<JobDef>.GetNamed("StakePawn");
                Action action = () =>
                {
                    Job job = new Job(killTarget, target);
                    job.count = 1;
                    pawn.jobs.TryTakeOrderedJob(job);
                };
                string text = "Stake pawn";
                FloatMenuOption menuOption = new FloatMenuOption(text, action, MenuOptionPriority.AttackEnemy, null, null, 0f, null, null);
                opts.Add(menuOption);


            }
        }
    }
}
