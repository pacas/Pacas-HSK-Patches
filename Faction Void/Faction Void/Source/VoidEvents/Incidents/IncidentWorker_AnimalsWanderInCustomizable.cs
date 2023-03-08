using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VoidEvents
{
    public class CustomizableAnimalsWanderInExtension : DefModExtension
    {
        public IntRange countRange;
        public List<BiomeAnimalRecord> animals;
        public FactionDef factionToSet;
        public MentalStateDef mentalStateDefToSet;
    }
    public class IncidentWorker_AnimalsWanderInCustomizable : IncidentWorker
    {
        public override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            Map map = (Map)parms.target;
            var extension = this.def.GetModExtension<CustomizableAnimalsWanderInExtension>();
            if (extension.factionToSet != null && Find.FactionManager.AllFactions
                .Any(x => x.def == extension.factionToSet && x.defeated is false) is false) 
            {
                return false;
            }
            if (RCellFinder.TryFindRandomPawnEntryCell(out var _, map, CellFinder.EdgeRoadChance_Animal))
            {
                return true;
            }
            return false;
        }

        public override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out var result, map, CellFinder.EdgeRoadChance_Animal))
            {
                return false;
            }
            var extension = this.def.GetModExtension<CustomizableAnimalsWanderInExtension>();
            int num = extension.countRange.RandomInRange;
            var pawns = new List<Pawn>();
            for (int i = 0; i < num; i++)
            {
                var kind = extension.animals.RandomElementByWeight(x => x.commonality).animal;
                var pawn = SpawnAnimal(result, map, kind);
                pawns.Add(pawn);
                if (extension.factionToSet != null)
                {
                    var faction = Find.FactionManager.FirstFactionOfDef(extension.factionToSet);
                    if (faction != null)
                    {
                        pawn.SetFaction(faction);
                    }
                }
                if (extension.mentalStateDefToSet != null)
                {
                    pawn.mindState.mentalStateHandler.TryStartMentalState(extension.mentalStateDefToSet);
                }
            }
            SendStandardLetter(parms, pawns);
            return true;
        }

        private Pawn SpawnAnimal(IntVec3 location, Map map, PawnKindDef pawnKind, Gender? gender = null)
        {
            IntVec3 loc = CellFinder.RandomClosewalkCellNear(location, map, 12);
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, null, 
                PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, 
                allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f,
                forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, 
                allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, 
                worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, gender));
            return GenSpawn.Spawn(pawn, loc, map, Rot4.Random) as Pawn;
        }
    }
}
