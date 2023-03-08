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
using Verse.Sound;

namespace VoidEvents
{
	public class GameCondition_VoidPlanetkiller : GameCondition
	{
		private const int SoundDuration = 179;

		private const int FadeDuration = 90;

		private static readonly Color FadeColor = Color.white;

		public override string TooltipString
		{
			get
			{
				Vector2 location = (Find.CurrentMap == null) ? default(Vector2) : Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile);
				return (string)(string.Concat(string.Concat(def.LabelCap, "\n"), "\n", Description) + ("\n" + "ImpactDate".Translate().CapitalizeFirst() + ": " + GenDate.DateFullStringAt(GenDate.TickGameToAbs(startTick + base.Duration), location))) + ("\n" + "TimeLeft".Translate().CapitalizeFirst() + ": " + base.TicksLeft.ToStringTicksToPeriod());
			}
		}

        public override void Init()
        {
            base.Init();
			this.TicksLeft = GenDate.TicksPerDay * 7;
        }
        public override void GameConditionTick()
		{
			base.GameConditionTick();
			if (base.TicksLeft <= 179)
			{
				Find.ActiveLesson.Deactivate();
				if (base.TicksLeft == 179)
				{
					SoundDefOf.PlanetkillerImpact.PlayOneShotOnCamera();
				}
				if (base.TicksLeft == 90)
				{
					ScreenFader.StartFade(FadeColor, 1f);
				}
			}
		}

		public override void End()
		{
			base.End();
			Impact();
		}

		public void EndNoImpact()
        {
			base.End();
			var voidFaction = Find.FactionManager.FirstFactionOfDef(VoidDefOf.RH_VOID);
			if (voidFaction != null)
			{
				voidFaction.defeated = true;
			}
			var hiddenFaction = Find.FactionManager.FirstFactionOfDef(VoidDefOf.RH2_Nerotonin4_Horde);
			if (hiddenFaction != null)
			{
				hiddenFaction.defeated = true;
			}
        }

		private void Impact()
		{
			ScreenFader.SetColor(Color.clear);
			GenGameEnd.EndGameDialogMessage("GameOverPlanetkillerImpact".Translate(Find.World.info.name), allowKeepPlaying: false, FadeColor);
		}
	}
}