using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace VoidEvents
{
    public class LordToil_VoidNegotiator_GoToAndContact : LordToil
    {
        public bool? pass;

        public IntVec3 spot;

        public Pawn negotiator;
        public LordToil_VoidNegotiator_GoToAndContact(Pawn negotiator, IntVec3 spot)
        {
            this.spot = spot;
            this.negotiator = negotiator;
        }
        public override void UpdateAllDuties()
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                if (negotiator == lord.ownedPawns[i])
                {
                    var pawnDuty = new PawnDuty(VoidDefOf.Void_MoveInPlace, spot)
                    {
                        locomotion = LocomotionUrgency.Walk
                    };
                    lord.ownedPawns[i].mindState.duty = pawnDuty;
                }
                else
                {
                    var pawnDuty = new PawnDuty(DutyDefOf.Follow, negotiator, 6)
                    {
                        locomotion = LocomotionUrgency.Walk
                    };
                    lord.ownedPawns[i].mindState.duty = pawnDuty;
                }
            }
        }

        public override void DrawPawnGUIOverlay(Pawn pawn)
        {
            if (pawn == negotiator)
            {
                pawn.Map.overlayDrawer.DrawOverlay(pawn, OverlayTypes.QuestionMark);
            }
        }
        public override IEnumerable<FloatMenuOption> ExtraFloatMenuOptions(Pawn target, Pawn forPawn)
        {
            if (target != negotiator)
            {
                yield break;
            }
            yield return new FloatMenuOption("Void.Contact".Translate(), delegate
            {
                forPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(VoidDefOf.Void_Contact, negotiator));
            });
        }
    }
}