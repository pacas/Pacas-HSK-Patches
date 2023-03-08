using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace VoidEvents
{
    public class JobDriver_Contact : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return new Toil
            {
                initAction = delegate
                {
                    var node = new DiaNode(VoidDefOf.VoidContact.voidStartingText);
                    var contacter = pawn.Faction.def == VoidDefOf.RH_VOID ? pawn : TargetA.Pawn;
                    Find.WindowStack.Add(new Dialog_VoidContact(contacter, node));
                }
            };
        }
    }
}