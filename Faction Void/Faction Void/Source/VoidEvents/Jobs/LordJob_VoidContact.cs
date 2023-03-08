using RimWorld;
using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Enlist;
namespace VoidEvents
{
    public class LordJob_VoidContact : LordJob_Joinable_Gathering
    {
        public const int ExpirationTicks = 30000;

        public const int WaitTimeTicks = 600;

        public Pawn negotiator;

        private LordToil exitToil;

        public IntVec3 spotCell;

        LordToil_AssaultColony lordToil_AssaultColony;
        public override bool AlwaysShowWeapon => true;
        public override IntVec3 Spot => spotCell;
        public override bool AllowStartNewGatherings => true;
        public LordJob_VoidContact()
        {
        }

        public LordJob_VoidContact(Pawn negotiator, IntVec3 spotCell)
        {
            this.negotiator = negotiator;
            this.spotCell = spotCell;
        }

        public override StateGraph CreateGraph()
        {
            var stateGraph = new StateGraph();
            var lordToil_MoveInPlace = new LordToil_VoidNegotiator_GoToAndContact(negotiator, spotCell);
            stateGraph.AddToil(lordToil_MoveInPlace);
            exitToil = new LordToil_ExitMap(LocomotionUrgency.Walk);
            stateGraph.AddToil(exitToil);

            lordToil_AssaultColony = new LordToil_AssaultColony();
            stateGraph.AddToil(lordToil_AssaultColony);
            var transition14 = new Transition(lordToil_MoveInPlace, lordToil_AssaultColony);
            transition14.AddTrigger(new Trigger_BecamePlayerEnemy());
            transition14.AddPostAction(new TransitionAction_Custom((Action)delegate
            {
                MakeVoidHostile();
            }));
            stateGraph.AddTransition(transition14);
            return stateGraph;
        }

        public override void Notify_InMentalState(Pawn pawn, MentalStateDef stateDef)
        {

        }

        public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
        {

        }

        public void MakeVoidHostile()
        {
            this.lord.GotoToil(lordToil_AssaultColony);
            this.lord.faction.ChangeRelation(-80);
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref negotiator, "negotiator");
            Scribe_Values.Look(ref spotCell, "spotCell");
        }

        public override LordToil CreateGatheringToil(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef)
        {
            throw new NotImplementedException();
        }
    }
}