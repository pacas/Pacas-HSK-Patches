using RimWorld;
using Verse;

namespace VoidEvents
{
    public class IncidentWorker_Stalker : IncidentWorker
    {
        public override bool CanFireNowSub(IncidentParms parms)
        {
            var voidFaction = Find.FactionManager.FirstFactionOfDef(VoidDefOf.RH_VOID);
            if (voidFaction is null || voidFaction.defeated)
            {
                return false;
            }
            Map map = (Map)parms.target;
            return RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 result, map, CellFinder.EdgeRoadChance_Animal);
        }
        public override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 result, map, CellFinder.EdgeRoadChance_Animal);
            IntVec3 loc = CellFinder.RandomClosewalkCellNear(result, map, 10);
            Pawn pawn = PawnGenerator.GeneratePawn(VoidDefOf.RH_DF2_Stalker);
            GenSpawn.Spawn(pawn, loc, map, Rot4.Random);
            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
            return true;
        }
    }
}
