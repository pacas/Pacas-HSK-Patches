using RimWorld;
using Verse;
using Verse.AI;

namespace VoidEvents
{
    public class JobGiver_SwallowNearestPawn : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            var comp = pawn.GetComp<CompSwallowedItems>();
            if (comp.Props.maxSwallowedItems > comp.innerContainer.Count)
            {
                var nearbyDownedPawn = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                    ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(pawn), 10, (Thing x) => x is Pawn victim
                    && victim.HostileTo(pawn) && victim.Downed && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly));
                if (nearbyDownedPawn != null)
                {
                    return JobMaker.MakeJob(VoidDefOf.Void_SwallowTarget, nearbyDownedPawn);
                }
                else
                {
                    var nearbyCorpse = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                            ThingRequest.ForGroup(ThingRequestGroup.Corpse), PathEndMode.OnCell, TraverseParms.For(pawn), 10,
                            (Thing x) => x is Corpse corpse && pawn.HostileTo(corpse.InnerPawn.Faction) && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly));
                    if (nearbyCorpse != null)
                    {
                        return JobMaker.MakeJob(VoidDefOf.Void_SwallowTarget, nearbyCorpse);
                    }
                }
            }
            return null;
        }
    }


}
