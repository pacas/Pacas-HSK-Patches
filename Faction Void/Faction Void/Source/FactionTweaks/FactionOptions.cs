using RimWorld;
using System.Collections.Generic;
using Verse;

namespace FactionTweaks
{
    public class FactionOptions : DefModExtension
    {
        public bool hideFactionLeader;
        public List<NameTriple> leaderNames = new List<NameTriple>();
        public List<PreceptDef> preceptsToAdd = new List<PreceptDef>();
        public IdeoIconDef customIdeoIcon;
        public ColorDef customIdeoColor;
        public string customIdeoDescription;
        public bool hasUniqueIdeo;
    }
}
