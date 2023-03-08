using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ScenarioAdditions
{
    [StaticConstructorOnStartup]
    internal static class HarmonyInit
    {
        static HarmonyInit()
        {
            new Harmony("CP.ScenarioAdditions").PatchAll();
        }
    }



	[HarmonyPatch(typeof(Pawn_DraftController), "GetGizmos")]
	public class Pawn_DraftController_GetGizmos_Patch
	{
		public static void Postfix(ref IEnumerable<Gizmo> __result, ref Pawn_DraftController __instance)
		{
			if (!__instance.Drafted || Find.CurrentMap == null || Find.World == null || Find.World.renderer == null || Find.World.renderer.wantedMode == WorldRenderMode.Planet)
			{
				return;
			}
			if (__result == null || !__result.Any<Gizmo>())
			{
				return;
			}
			Pawn pawn = __instance.pawn;
			List<Gizmo> list = __result.ToList<Gizmo>();
			foreach (var part in Find.Scenario.AllParts.Where(x => x is ScenPart_CallForReinforcements).Cast<ScenPart_CallForReinforcements>())
            {
				var factionToCall = Find.FactionManager.FirstFactionOfDef(part.factionRequirement.faction);
				Command_Action item = new Command_Action
				{
					defaultLabel = part.gizmoLabel,
					defaultDesc = part.gizmoDesc,
					icon = ContentFinder<Texture2D>.Get(part.gizmoIconTexPath),
					disabled = !CanCall(part, factionToCall),
					action = delegate
					{
						IncidentParms incidentParms = new IncidentParms();
						incidentParms.target = Find.CurrentMap;
						incidentParms.faction = factionToCall;
						incidentParms.raidArrivalModeForQuickMilitaryAid = true;
						incidentParms.points = DiplomacyTuning.RequestedMilitaryAidPointsRange.RandomInRange;
						factionToCall.lastMilitaryAidRequestTick = Find.TickManager.TicksGame;
						IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms);
					}
				};
				list.Add(item);
			}
			__result = list;
		}

		private static bool CanCall(ScenPart_CallForReinforcements scenPart, Faction factionToCall)
		{
			if (factionToCall is null)
            {
				return false;
            }
			if (scenPart.cooldownTicks != -1 && Find.TickManager.TicksGame < factionToCall.lastMilitaryAidRequestTick + scenPart.cooldownTicks)
            {
				return false;
            }
			if (scenPart.factionRequirement != null && factionToCall.GoodwillWith(Faction.OfPlayer) < scenPart.factionRequirement.goodwill)
			{
				return false;
			}
			return true;
		}
	}
}
