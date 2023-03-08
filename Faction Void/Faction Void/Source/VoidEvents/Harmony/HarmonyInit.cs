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
    [StaticConstructorOnStartup]
    static class HarmonyContainer
    {
        static HarmonyContainer()
        {
            Harmony harmony = new Harmony("com.dninemfive.chickenVOID");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryExecuteWorker", new Type[] { typeof(IncidentParms) })]
        class VOIDPatch
        {
            [HarmonyPrefix]
            public static bool TryExecuteWorkerPrefix(ref IncidentParms parms)
            {
                Extension ext;
                if ((ext = parms.faction?.def.GetModExtension<Extension>()) != null)
                {
                    if (ext.arrivalMode != null) parms.raidArrivalMode = ext.arrivalMode;
                    if (ext.strategy != null) parms.raidStrategy = ext.strategy;
                    var map = (parms.target as Map);
                    if (ext.incidentsOnArrival != null && map.ParentFaction.def != VoidDefOf.RH_VOID)
                    {
                        foreach (var inc in ext.incidentsOnArrival)
                        {
                            inc.Worker.TryExecute(StorytellerUtility.DefaultParmsNow(inc.category, parms.target));
                        }
                        //if (parms.faction.def == VoidDefOf.RH_VOID)
                        //{
                        //    var autoMortar = ThingMaker.MakeThing(ThingDef.Named("Turret_AutoMortar"));
                        //    autoMortar.SetFaction(parms.faction);
                        //    if (CellFinder.TryFindRandomEdgeCellWith(x => x.Walkable(map)
                        //    && x.Roofed(map) is false, map, 0, out var cell))
                        //    {
                        //        DropPodUtility.DropThingsNear(cell, map, Gen.YieldSingle(autoMortar));
                        //    }
                        //}
                    }
                }
                return true;
            }
        }
    }
    public class Extension : DefModExtension
    {
#pragma warning disable CS0649
        public PawnsArrivalModeDef arrivalMode;
        public List<IncidentDef> incidentsOnArrival;
        public RaidStrategyDef strategy;
#pragma warning restore CS0649
    }


}