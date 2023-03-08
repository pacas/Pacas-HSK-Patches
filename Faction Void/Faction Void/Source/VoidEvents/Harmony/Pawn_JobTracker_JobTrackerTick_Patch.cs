using HarmonyLib;
using Verse.AI;

namespace VoidEvents
{
    [HarmonyPatch(typeof(Pawn_JobTracker), "JobTrackerTick")]
    public static class Pawn_JobTracker_JobTrackerTick_Patch
    {
        public static bool Prefix(Pawn_JobTracker __instance)
        {
            if (__instance.pawn.Spawned is false)
            {
                return false;
            }
            return true;
        }
    }
}