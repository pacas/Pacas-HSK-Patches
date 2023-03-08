using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI.Group;

namespace VoidEvents
{
    [HarmonyPatch(typeof(Settlement), "PostRemove")]
    class PostRemove_Patch
    {
        public static void Prefix(Settlement __instance)
        {
            if (__instance.Faction?.def == VoidDefOf.RH_VOID 
                && !Find.World.worldObjects.Settlements.Where(x => x.Faction?.def == VoidDefOf.RH_VOID).Any())
            {
                var parms = StorytellerUtility.DefaultParmsNow(VoidDefOf.Void_PlanetKiller.category, Find.AnyPlayerHomeMap);
                parms.faction = __instance.Faction;
                Find.Storyteller.incidentQueue.Add(VoidDefOf.Void_PlanetKiller, Find.TickManager.TicksGame + GenDate.TicksPerDay * 3, parms);
            }
        }
    }
}