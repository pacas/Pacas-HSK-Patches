using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace VoidEvents
{
	public class SitePartWorker_NerotoninStash : SitePartWorker
	{
		public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
		{
			base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
			var amount = Rand.RangeInclusive(10, 50);
			List<Thing> list = new List<Thing>();
			var nerotonin = ThingMaker.MakeThing(VoidDefOf.RH_Nerotonin8B);
			nerotonin.stackCount = amount;
			list.Add(nerotonin);
			part.things = new ThingOwner<Thing>(part, oneStackOnly: false);
			part.things.TryAddRangeOrTransfer(list, canMergeWithExistingStacks: false);
			slate.Set("generatedItemStashThings", list);
			outExtraDescriptionRules.Add(new Rule_String("itemStashContents", GenLabel.ThingsLabel(list)));
			outExtraDescriptionRules.Add(new Rule_String("itemStashContentsValue", GenThing.GetMarketValue(list).ToStringMoney()));
		}

		public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
		{
			string text = base.GetPostProcessedThreatLabel(site, sitePart);
			if (site.HasWorldObjectTimeout)
			{
				text += " (" + "DurationLeft".Translate(site.WorldObjectTimeoutTicksLeft.ToStringTicksToPeriod()) + ")";
			}
			return text;
		}

        public override void SitePartWorkerTick(SitePart sitePart)
        {
            base.SitePartWorkerTick(sitePart);
			if (sitePart.site.Map != null)
            {
				foreach (var lord in sitePart.site.Map.lordManager.lords)
                {
					if (lord.LordJob is LordJob_DefendBase defendBase && lord.faction.HostileTo(Faction.OfPlayer) && lord.Graph.lordToils.IndexOf(lord.CurLordToil) >= 2)
                    {
						var gameComp = Current.Game.GetComponent<VoidGameComp>();
						if (!gameComp.voidReinforcementsToSend.ContainsKey(sitePart.site.Map))
                        {
							gameComp.voidReinforcementsToSend[sitePart.site.Map] = Find.TickManager.TicksAbs + new IntRange(1 * GenDate.TicksPerHour, 3 * GenDate.TicksPerHour).RandomInRange;
						}
					}
                }
            }
        }
    }
}
