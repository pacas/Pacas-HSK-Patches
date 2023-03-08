using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ScenarioAdditions
{
	public class FactionRelations : IExposable
    {
		public FactionDef faction;
		public int goodwill;
		public FactionRelations()
        {

        }

        public void ExposeData()
        {
			Scribe_Defs.Look(ref faction, "faction");
			Scribe_Values.Look(ref goodwill, "goodwill");
        }
    }
	internal class ScenPart_DropPodSupplies : ScenPart
	{
		private float intervalDays;

		private bool repeat;

		private float occurTick;

		private bool isFinished;

		protected FactionRelations factionRequirement;

		protected ThingDef thingDef;

		protected ThingDef stuff;

		protected int count = 1;

		private string countBuf;
		private float IntervalTicks => 60000f * intervalDays;

		private string intervalDaysBuffer;

		private string DropSupplyTag = "DropSupply";

		protected string letterLabelDropSupply;
		protected string letterDescDropSupply;
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref intervalDays, "intervalDays", 0f);
			Scribe_Values.Look(ref repeat, "repeat", defaultValue: false);
			Scribe_Values.Look(ref occurTick, "occurTick", 0f);
			Scribe_Values.Look(ref isFinished, "isFinished", defaultValue: false);
			Scribe_Defs.Look(ref thingDef, "thingDef");
			Scribe_Defs.Look(ref stuff, "stuff");
			Scribe_Values.Look(ref count, "count");
			Scribe_Deep.Look(ref factionRequirement, "factionRequirement");
		}

		public override string Summary(Scenario scen)
		{
			return ScenSummaryList.SummaryWithList(scen, DropSupplyTag, "DropSupplies".Translate());
		}

		public override IEnumerable<string> GetSummaryListEntries(string tag)
		{
			if (tag == DropSupplyTag)
			{
				yield return thingDef.LabelCap + (stuff != null ? " (" + stuff.label + ") " : "") + " x" + count;
			}
		}

		public override bool TryMerge(ScenPart other)
		{
			return false;
		}

		public override bool CanCoexistWith(ScenPart other)
		{
			return true;
		}


		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, (24 * 5) + 3 * 4);
			Rect rect = new Rect(scenPartRect.x, scenPartRect.y, scenPartRect.width, 24f);
			Rect rect2 = new Rect(scenPartRect.x, rect.yMax + 3, scenPartRect.width, 24f);
			Rect rect3 = new Rect(scenPartRect.x, rect2.yMax + 3, scenPartRect.width, 24f);
			Rect rect4 = new Rect(scenPartRect.x, rect3.yMax + 3, scenPartRect.width, 24f);
			Rect rect5 = new Rect(scenPartRect.x, rect4.yMax + 3, scenPartRect.width, 24f);

			if (Widgets.ButtonText(rect, thingDef.LabelCap))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (ThingDef item in from t in DefDatabase<ThingDef>.AllDefs.Where(x => x.mote == null && x.building == null && !x.IsCorpse && !x.IsBlueprint
										  && (!x.thingCategories?.Contains(ThingCategoryDefOf.PlantFoodRaw) ?? false)
										  && (!x.thingCategories?.Contains(ThingCategoryDefOf.Chunks) ?? false))
										  orderby t.label
										  select t)
				{
					ThingDef localTd = item;
					list.Add(new FloatMenuOption(localTd.LabelCap, delegate
					{
						thingDef = localTd;
						stuff = GenStuff.DefaultStuffFor(localTd);
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			if (thingDef.MadeFromStuff && Widgets.ButtonText(rect2, stuff.LabelCap))
			{
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				foreach (ThingDef item2 in from t in GenStuff.AllowedStuffsFor(thingDef)
										   orderby t.label
										   select t)
				{
					ThingDef localSd = item2;
					list2.Add(new FloatMenuOption(localSd.LabelCap, delegate
					{
						stuff = localSd;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list2));
			}

			Widgets.TextFieldNumeric(rect3, ref count, ref countBuf, 1f);
			Widgets.TextFieldNumericLabeled(rect4, "intervalDays".Translate(), ref intervalDays, ref intervalDaysBuffer);
			Widgets.CheckboxLabeled(rect5, "repeat".Translate(), ref repeat);
		}

		public override void Randomize()
		{
			base.Randomize();
			intervalDays = 15f * Rand.Gaussian() + 30f;
			if (intervalDays < 0f)
			{
				intervalDays = 0f;
			}
			repeat = (Rand.Range(0, 100) < 50);
			thingDef = ThingDefOf.Pemmican;
		}
		public override void PostGameStart()
		{
			base.PostGameStart();
			occurTick = (float)Find.TickManager.TicksGame + IntervalTicks;
		}

		public override void Tick()
		{
			base.Tick();
			if (Find.AnyPlayerHomeMap == null || isFinished)
			{
				return;
			}
			if (thingDef == null)
			{
				Log.Error("Trying to tick ScenPart_CreateIncident but the incident is null");
				isFinished = true;
			}
			else if ((float)Find.TickManager.TicksGame >= occurTick)
			{
				if (this.factionRequirement != null)
                {
					var faction = Find.FactionManager.FirstFactionOfDef(this.factionRequirement.faction);
					if (faction != null)
                    {
						if (faction.GoodwillWith(Faction.OfPlayer) < this.factionRequirement.goodwill)
                        {
							if (repeat && intervalDays > 0f)
							{
								occurTick += IntervalTicks;
							}
							return;
                        }
                    }
                }
				var supplies = GenerateSupplies();
				if (!supplies.Any())
				{
					isFinished = true;
					return;
				}
				else if (repeat && intervalDays > 0f)
				{
					occurTick += IntervalTicks;
				}
				else
				{
					isFinished = true;
				}
				var map = Find.Maps.Where((Map x) => x.IsPlayerHome).RandomElement();
				var cell = DropCellFinder.RandomDropSpot(map);
				DropPodUtility.DropThingsNear(cell, map, supplies);
				SendStandardLetter(letterLabelDropSupply ?? "DropSuppliesLabel".Translate(), letterDescDropSupply ?? "DropSuppliesDesc".Translate(), LetterDefOf.PositiveEvent, new TargetInfo(cell, map));
			}
		}


		protected void SendStandardLetter(TaggedString baseLetterLabel, TaggedString baseLetterText, LetterDef baseLetterDef, LookTargets lookTargets, params NamedArgument[] textArgs)
		{
			ChoiceLetter choiceLetter = LetterMaker.MakeLetter(baseLetterLabel, baseLetterText.Formatted(textArgs), baseLetterDef, lookTargets);
			Find.LetterStack.ReceiveLetter(choiceLetter);
		}

		private List<Thing> GenerateSupplies()
		{
			var supplies = new List<Thing>();
			var supply = ThingMaker.MakeThing(thingDef, stuff);
			supply.stackCount = count;
			supplies.Add(supply);
			return supplies;
		}
	}
}
