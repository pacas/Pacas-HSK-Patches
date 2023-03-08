using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Reflection;
using Verse;

namespace VoidEvents
{
    [HarmonyPatch]
    public static class VFEMechPatch
    {
        public static MethodBase targetMethod;

        [HarmonyPrepare]
        public static bool Prepare()
        {
            var type = AccessTools.TypeByName("VFEMech.TravelingMissile");
            if (type != null)
            {
                targetMethod = AccessTools.Method(type, "Arrived");
            }
            return targetMethod != null;
        }
        [HarmonyTargetMethod]
        public static MethodBase GetMethod()
        {
            return targetMethod;
        }

        public static bool Prefix(WorldObject __instance)
        {
            var destinationTile = Traverse.Create(__instance).Field("destinationTile").GetValue<int>();
            foreach (var worldObject in Find.WorldObjects.ObjectsAt(destinationTile))
            {
                if (worldObject is Settlement settlement && settlement.Faction.def == VoidDefOf.RH_VOID || worldObject is VoidCamp voidCamp)
                {
                    Messages.Message("Void.NukeIntercepted".Translate(), worldObject, MessageTypeDefOf.CautionInput);
                    __instance.Destroy();
                    return false;
                }
            }
            return true;
        }
    }
}