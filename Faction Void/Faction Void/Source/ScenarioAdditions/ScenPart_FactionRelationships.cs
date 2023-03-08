using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ScenarioAdditions
{
    public class ScenPart_FactionRelationships : ScenPart
    {
        public List<FactionRelations> factionRelationsGains = new List<FactionRelations>();
        public List<FactionDef> allEnemiesExcept = new List<FactionDef>();
        public override void GenerateIntoMap(Map map)
        {
            base.GenerateIntoMap(map);
            foreach (FactionRelations factionRel in factionRelationsGains)
            {
                Faction faction = Find.FactionManager.FirstFactionOfDef(factionRel.faction);
                faction.TryAffectGoodwillWith(Faction.OfPlayer, factionRel.goodwill, false, false);
            }
            if (allEnemiesExcept.Count > 0)
            {
                foreach (Faction faction in Find.FactionManager.AllFactions)
                {
                    if (!allEnemiesExcept.Contains(faction.def) && !faction.IsPlayer)
                    {
                        FactionRelation relation = new FactionRelation
                        {
                            other = Faction.OfPlayer,
                            baseGoodwill = -100,
                            kind = FactionRelationKind.Hostile
                        };
                        faction.SetRelation(relation);
                    }
                }
            }
        }
    }
}
