using RimWorld;
using System.Linq;
using Verse;
using Verse.AI;

namespace VoidEvents
{
    public class JobGiver_TakeVoidPrisonerWhenClose : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            if (!RCellFinder.TryFindBestExitSpot(pawn, out IntVec3 spot))
            {
                return null;
            }
            var prisoner = pawn.Map.mapPawns.PrisonersOfColony.Where(x => x.Faction == pawn.Faction && x.guest.PrisonerIsSecure
                && x.Position.DistanceTo(pawn.Position) < 30 && pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly)).FirstOrDefault();
            if (prisoner != null)
            {
                if (prisoner.Downed)
                {
                    Job job = JobMaker.MakeJob(JobDefOf.Steal);
                    job.targetA = prisoner;
                    job.locomotionUrgency = LocomotionUrgency.Jog;
                    job.targetB = spot;
                    job.count = 1;
                    return job;
                }
                else
                {
                    if (!RCellFinder.TryFindPrisonerReleaseCell(prisoner, pawn, out var result))
                    {
                        return null;
                    }
                    Job job = JobMaker.MakeJob(VoidDefOf.Void_ReleasePrisoner, prisoner, result);
                    job.count = 1;
                    return job;
                }
            }
            return null;
        }
    }
}