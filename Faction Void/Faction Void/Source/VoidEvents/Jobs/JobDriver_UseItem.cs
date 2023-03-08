using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI.Group;
using Verse.AI;

namespace VoidEvents
{
    public class JobDriver_UseItem : JobDriver
    {
        private int useDuration = 40;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref useDuration, "useDuration", 0);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            this.job.count = 1;
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            Toil toil = Toils_General.Wait(useDuration);
            toil.WithProgressBarToilDelay(TargetIndex.A);
            yield return toil;
            Toil use = new Toil();
            use.initAction = delegate
            {
                Pawn actor = use.actor;
                Pawn animal = (Pawn)job.targetB.Thing;
                animal.SetFaction(Faction.OfPlayer);
                if (!animal.health.hediffSet.HasHediff(VoidDefOf.Void_SecronomControlChip))
                {
                	var hediff = HediffMaker.MakeHediff(VoidDefOf.Void_SecronomControlChip, animal);
                    animal.health.AddHediff(hediff);
                }
                foreach (var t in TrainableUtility.TrainableDefsInListOrder)
                {
                    animal.training.SetWantedRecursive(t, true);
                    animal.training.Train(t, actor, true);
                }
                job.targetA.Thing.Destroy();
            };
            use.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return use;
        }
    }
}
