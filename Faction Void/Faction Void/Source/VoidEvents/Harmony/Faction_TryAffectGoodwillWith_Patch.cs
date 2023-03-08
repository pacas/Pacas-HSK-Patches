using HarmonyLib;
using RimWorld;

namespace VoidEvents
{
    [HarmonyPatch(typeof(Faction), "TryAffectGoodwillWith")]
    public static class Faction_TryAffectGoodwillWith_Patch
    {
        public static bool Prefix(Faction __instance, Faction other, int goodwillChange)
        {
            if (goodwillChange > 0 && (other != null && other.def == VoidDefOf.RH_VOID || __instance.def == VoidDefOf.RH_VOID))
            {
                if ((__instance == Faction.OfPlayer || other == Faction.OfPlayer) && VoidGameComp.IsEnlistedToVoid())
                {
                    return true;
                }
                return false;
            }
            return true;
        }
    }
}