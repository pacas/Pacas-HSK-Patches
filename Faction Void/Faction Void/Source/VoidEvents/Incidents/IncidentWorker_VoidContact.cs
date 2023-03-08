using Enlist;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace VoidEvents
{
    public class IncidentWorker_VoidContact : IncidentWorker
    {
        public override bool CanFireNowSub(IncidentParms parms)
        {
            return true;
        }

        public static bool ShouldExecute(IncidentParms parms)
        {
            var comp = Current.Game.GetComponent<VoidGameComp>();
            if (VoidGameComp.IsEnlistedToVoid())
            {
                return false;
            }
            if (GenDate.DaysPassed < 1)
            {
                return false;
            }
            if (comp.voidVisited)
            {
                return false;
            }
            var localMap = parms.target as Map;
            if (localMap == null)
            {
                return false;
            }
            var voidFaction = parms.faction;
            if (voidFaction == null)
            {
                return false;
            }
            return true;
        }
        public override bool TryExecuteWorker(IncidentParms parms)
        {
            var comp = Current.Game.GetComponent<VoidGameComp>();
            comp.voidVisited = true;
            var localMap = parms.target as Map;
            if (parms.faction is null)
            {
                parms.faction = Find.FactionManager.FirstFactionOfDef(VoidDefOf.RH_VOID);
            }
            var voidFaction = parms.faction;
            if (voidFaction.HostileTo(Faction.OfPlayer))
            {
                voidFaction.ChangeRelation(0);
            }
            List<Pawn> pawns = new List<Pawn>();
            Pawn negotiator = PawnGenerator.GeneratePawn(VoidDefOf.VoidContact.negotiatorDef, voidFaction);
            pawns.Add(negotiator);
            for (int count = 0; count < VoidDefOf.VoidContact.bodyguardCount; count++)
            {
                Pawn bodyGuard = PawnGenerator.GeneratePawn(VoidDefOf.VoidContact.bodyguardDef, voidFaction);
                pawns.Add(bodyGuard);
            }
            if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Standable(localMap) && localMap.reachability.CanReachColony(x),
                localMap, CellFinder.EdgeRoadChance_Hostile, out IntVec3 entry))
            {
                if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(localMap) && localMap.reachability.CanReachColony(c),
                    localMap, CellFinder.EdgeRoadChance_Hostile, out entry)
                    && !CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(localMap), localMap,
                    CellFinder.EdgeRoadChance_Hostile, out entry))
                {
                    entry = DropCellFinder.TradeDropSpot(localMap);
                }
            };
            for (int i = 0; i < pawns.Count; i++)
            {
                IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(entry, localMap);
                GenSpawn.Spawn(pawns[i], loc, localMap, Rot4.Random);
            }

            RCellFinder.TryFindRandomSpotJustOutsideColony(pawns[0].Position, pawns[0].MapHeld, pawns[0], out IntVec3 destSpot, delegate (IntVec3 c)
            {
                for (int k = 0; k < pawns.Count; k++)
                {
                    if (!pawns[k].CanReach(c, PathEndMode.OnCell, Danger.Deadly))
                    {
                        return false;
                    }
                }
                return true;
            });

            Lord lord = LordMaker.MakeNewLord(voidFaction, new LordJob_VoidContact(negotiator, destSpot), localMap, pawns);
            Find.LetterStack.ReceiveLetter("Void.VoidContact".Translate(), "Void.VoidVisitorsDesc".Translate(), LetterDefOf.NewQuest, pawns);
            return true;
        }
    }
}