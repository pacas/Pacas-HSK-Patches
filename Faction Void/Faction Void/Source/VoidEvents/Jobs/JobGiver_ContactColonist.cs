using System.Linq;
using Verse;
using Verse.AI;

namespace VoidEvents
{
    public class JobGiver_ContactColonist : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            var colonistToContact = pawn.Map.mapPawns.FreeColonistsSpawned.Where(x => pawn.CanReserveAndReach(x, PathEndMode.InteractionCell, Danger.Deadly));
            if (colonistToContact.TryRandomElement(out var colonist))
            {
                return JobMaker.MakeJob(VoidDefOf.Void_Contact, colonist);
            }
            return null;
        }
    }
}