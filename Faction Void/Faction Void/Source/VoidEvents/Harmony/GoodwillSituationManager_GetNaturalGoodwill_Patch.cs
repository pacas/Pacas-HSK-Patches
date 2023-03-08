using HarmonyLib;
using RimWorld;

namespace VoidEvents
{
    [HarmonyPatch(typeof(GoodwillSituationManager), "GetNaturalGoodwill")]
    public static class GoodwillSituationManager_GetNaturalGoodwill_Patch
    {
        public static void Postfix(ref int __result, Faction other)
        {
            if (other != null && other.def == VoidDefOf.RH_VOID)
            {
                __result = 0;
            }
        }
    }
}