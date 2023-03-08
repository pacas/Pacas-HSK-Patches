using HarmonyLib;
using RimWorld.Planet;

namespace VoidEvents
{
    [HarmonyPatch(typeof(Settlement), "Visitable", MethodType.Getter)]
    class Visitable_Patch
    {
        public static void Postfix(Settlement __instance, ref bool __result)
        {
            if (__instance.Faction?.def == VoidDefOf.RH_VOID)
            {
                __result = true;
            }
        }
    }
}