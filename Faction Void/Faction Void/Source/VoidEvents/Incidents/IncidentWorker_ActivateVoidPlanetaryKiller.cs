using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using RimWorld.Planet;

namespace VoidEvents
{
    public class IncidentWorker_ActivateVoidPlanetaryKiller : IncidentWorker
    {
        public int FindTileAtIceSheet()
        {
            var list = new List<int>();
            for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
            {
                if (Find.WorldGrid[i].biome == BiomeDefOf.IceSheet)
                {
                    list.Add(i);
                }
            }
            if (list.TryRandomElement(out var tile))
            {
                return tile;
            }
            if (TileFinder.TryFindNewSiteTile(out tile, 30))
            {
                return tile;
            }
            if (TileFinder.TryFindNewSiteTile(out tile))
            {
                return tile;
            }
            return Rand.RangeInclusive(1, Find.WorldGrid.TilesCount - 1);
        }
        public override bool TryExecuteWorker(IncidentParms parms)
        {
            var tile = FindTileAtIceSheet();
            parms.faction = Find.FactionManager.FirstFactionOfDef(VoidDefOf.RH_VOID);
            var worldObject = VoidUtils.MakeSite(VoidDefOf.Void_PlanetaryKillerSite, VoidDefOf2.Void_PlanetaryKillerSite, tile, parms.faction);
            Find.WorldObjects.Add(worldObject);
            var part = worldObject.parts.FirstOrDefault(x => x.def == VoidDefOf2.Void_PlanetaryKillerSite);
            part.parms.threatPoints = 2000;
            GameConditionManager gameConditionManager = Find.World.GameConditionManager;
            GameCondition gameCondition = GameConditionMaker.MakeCondition(duration: Mathf.RoundToInt(def.durationDays.RandomInRange * 60000f), def: def.gameCondition);
            gameConditionManager.RegisterCondition(gameCondition);
            parms.letterHyperlinkThingDefs = gameCondition.def.letterHyperlinks;
            SendStandardLetter(def.letterLabel, gameCondition.LetterText, def.letterDef, parms, worldObject);
            return true;
        }
    }
}