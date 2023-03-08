using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace VoidEvents
{
    public class JobDriver_ReleasePrisoner : JobDriver
    {
        private Pawn Prisoner => (Pawn)job.GetTarget(TargetIndex.A).Thing;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Prisoner, job, 1, -1, null, errorOnFailed);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.B);
            this.FailOnDowned(TargetIndex.A);
            this.FailOnAggroMentalState(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
                .FailOn(() => !Prisoner.IsPrisonerOfColony 
                || !Prisoner.guest.PrisonerIsSecure)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, storageMode: false);
            Toil setReleased = ToilMaker.MakeToil("MakeNewToils");
            setReleased.initAction = delegate
            {
                Pawn pawn = setReleased.actor.jobs.curJob.targetA.Thing as Pawn;
                GenGuest.PrisonerRelease(pawn);
            };
            yield return setReleased;
        }
    }
}