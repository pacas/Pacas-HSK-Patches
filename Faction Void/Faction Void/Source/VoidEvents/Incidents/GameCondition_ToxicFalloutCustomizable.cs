using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VoidEvents
{
	public class CustomizableToxicFalloutExtension : DefModExtension
	{
		public int damageCheckInterval;
		public float plantKillChance;
		public float corpseRotProgressAdd;
		public float maxSkyLerpFactor;
		public float skyGlow;
		public Color colorSky;
		public Color colorShadow;
		public Color colorOverlay;
		public float saturation;
		public int transitionTicks;
		public float baseToxicBuildup;
		public float animalDensityFactor;
		public float plantDensityFactor;
	}
	public class GameCondition_ToxicFalloutCustomizable : GameCondition
	{
		public CustomizableToxicFalloutExtension Extension => def.GetModExtension<CustomizableToxicFalloutExtension>();
		private SkyColorSet ToxicFalloutColors => new SkyColorSet(Extension.colorSky, Extension.colorShadow, Extension.colorOverlay, Extension.saturation);

		private readonly List<SkyOverlay> overlays = new List<SkyOverlay>
		{
			new WeatherOverlay_Fallout()
		};
		public override int TransitionTicks => Extension.transitionTicks;

		public override void Init()
		{
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Critical);
		}

		public override void GameConditionTick()
		{
			List<Map> affectedMaps = base.AffectedMaps;
			if (Find.TickManager.TicksGame % Extension.damageCheckInterval == 0)
			{
				for (int i = 0; i < affectedMaps.Count; i++)
				{
					DoPawnsToxicDamage(affectedMaps[i]);
				}
			}
			for (int j = 0; j < overlays.Count; j++)
			{
				for (int k = 0; k < affectedMaps.Count; k++)
				{
					overlays[j].TickOverlay(affectedMaps[k]);
				}
			}
		}

		private void DoPawnsToxicDamage(Map map)
		{
			List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				DoPawnToxicDamage(allPawnsSpawned[i]);
			}
		}

		public void DoPawnToxicDamage(Pawn p)
		{
			if ((!p.Spawned || !p.Position.Roofed(p.Map)) && p.RaceProps.IsFlesh)
			{
				float num = 0.0230066683f;
				num *= Mathf.Max(1f - p.GetStatValue(StatDefOf.ToxicResistance), 0f);
				if (ModsConfig.BiotechActive)
				{
					num *= Mathf.Max(1f - p.GetStatValue(StatDefOf.ToxicEnvironmentResistance), 0f);
				}
				if (num != 0f)
				{
					float num2 = Mathf.Lerp(0.85f, 1.15f, Rand.ValueSeeded(p.thingIDNumber ^ 0x46EDC5D));
					num *= num2;
					HealthUtility.AdjustSeverity(p, HediffDefOf.ToxicBuildup, num);
				}
			}
		}

		public override void DoCellSteadyEffects(IntVec3 c, Map map)
		{
			if (c.Roofed(map))
			{
				return;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing is Plant)
				{
					if (thing.def.plant.dieFromToxicFallout && Rand.Value < Extension.plantKillChance)
					{
						thing.Kill();
					}
				}
				else if (thing.def.category == ThingCategory.Item)
				{
					CompRottable compRottable = thing.TryGetComp<CompRottable>();
					if (compRottable != null && (int)compRottable.Stage < 2)
					{
						compRottable.RotProgress += Extension.corpseRotProgressAdd;
					}
				}
			}
		}

		public override void GameConditionDraw(Map map)
		{
			for (int i = 0; i < overlays.Count; i++)
			{
				overlays[i].DrawOverlay(map);
			}
		}

		public override float SkyTargetLerpFactor(Map map)
		{
			return GameConditionUtility.LerpInOutValue(this, TransitionTicks, Extension.maxSkyLerpFactor);
		}

		public override SkyTarget? SkyTarget(Map map)
		{
			return new SkyTarget(Extension.skyGlow, ToxicFalloutColors, 1f, 1f);
		}

		public override float AnimalDensityFactor(Map map)
		{
			return Extension.animalDensityFactor;
		}

		public override float PlantDensityFactor(Map map)
		{
			return Extension.plantDensityFactor;
		}

		public override bool AllowEnjoyableOutsideNow(Map map)
		{
			return false;
		}

		public override List<SkyOverlay> SkyOverlays(Map map)
		{
			return overlays;
		}
	}
}
