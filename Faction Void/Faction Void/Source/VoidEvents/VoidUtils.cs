using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace VoidEvents
{
	public static class VoidUtils
	{
		public static void ActivateProtocol()
		{
            Current.Game.GetComponent<VoidGameComp>().argentProtocolTicks = Find.TickManager.TicksGame + GenDate.TicksPerDay;
            Find.LetterStack.ReceiveLetter("Void.ProjectArgentTitle".Translate(), "Void.ProjectArgentText".Translate(), LetterDefOf.NeutralEvent);
        }
		public static bool TryGetChangedLifeExpectancy(this Pawn pawn, out float newLifeExpectancy)
		{
			foreach (var hediff in pawn.health.hediffSet.hediffs)
			{
				var comp = hediff.TryGetComp<HediffCompLifeExpectancy>();
				if (comp != null)
				{
					newLifeExpectancy = comp.LifeExpectancy;
					return true;
				}
			}
			newLifeExpectancy = 0f;
			return false;
		}
        public static Site GenerateSite(WorldObjectDef def, SitePartDef sitePartDef, int tile, Faction faction, bool hiddenSitePartsPossible = false, RulePack singleSitePartRules = null)
		{
			var site = MakeSite(def, sitePartDef, tile, faction);
			var list = new List<Rule>();
			var dictionary = new Dictionary<string, string>();
			var list2 = new List<string>();
			int num = 0;
			for (int i = 0; i < site.parts.Count; i++)
			{
				var list3 = new List<Rule>();
				var dictionary2 = new Dictionary<string, string>();
				site.parts[i].def.Worker.Notify_GeneratedByQuestGen(site.parts[i], QuestGen.slate, list3, dictionary2);
				if (site.parts[i].hidden)
				{
					continue;
				}
				if (singleSitePartRules != null)
				{
					var list4 = new List<Rule>();
					list4.AddRange(list3);
					list4.AddRange(singleSitePartRules.Rules);
					string text = QuestGenUtility.ResolveLocalText(list4, dictionary2, "root", capitalizeFirstSentence: false);
					list.Add(new Rule_String("sitePart" + num + "_description", text));
					if (!text.NullOrEmpty())
					{
						list2.Add(text);
					}
				}
				for (int j = 0; j < list3.Count; j++)
				{
					var rule = list3[j].DeepCopy();
					if (rule is Rule_String rule_String && num != 0)
					{
						rule_String.keyword = "sitePart" + num + "_" + rule_String.keyword;
					}
					list.Add(rule);
				}
				foreach (var item in dictionary2)
				{
					string text2 = item.Key;
					if (num != 0)
					{
						text2 = "sitePart" + num + "_" + text2;
					}
					if (!dictionary.ContainsKey(text2))
					{
						dictionary.Add(text2, item.Value);
					}
				}
				num++;
			}
			if (!list2.Any())
			{
				list.Add(new Rule_String("allSitePartsDescriptions", "HiddenOrNoSitePartDescription".Translate()));
				list.Add(new Rule_String("allSitePartsDescriptionsExceptFirst", "HiddenOrNoSitePartDescription".Translate()));
			}
			else
			{
				list.Add(new Rule_String("allSitePartsDescriptions", list2.ToClauseSequence().Resolve()));
				if (list2.Count >= 2)
				{
					list.Add(new Rule_String("allSitePartsDescriptionsExceptFirst", list2.Skip(1).ToList().ToClauseSequence()));
				}
				else
				{
					list.Add(new Rule_String("allSitePartsDescriptionsExceptFirst", "HiddenOrNoSitePartDescription".Translate()));
				}
			}
			QuestGen.AddQuestDescriptionRules(list);
			QuestGen.AddQuestNameRules(list);
			QuestGen.AddQuestDescriptionConstants(dictionary);
			QuestGen.AddQuestNameConstants(dictionary);
			QuestGen.AddQuestNameRules(new List<Rule>
			{
				new Rule_String("site_label", site.Label)
			});
			return site;
		}

		public static Site MakeSite(WorldObjectDef worldObjectDef, SitePartDef sitePartDef, int tile, Faction faction, bool ifHostileThenMustRemainHostile = true)
		{
			var site = (Site)WorldObjectMaker.MakeWorldObject(worldObjectDef);
			site.Tile = tile;
			site.SetFaction(faction);
			if (ifHostileThenMustRemainHostile && faction != null && faction.HostileTo(Faction.OfPlayer))
			{
				site.factionMustRemainHostile = true;
			}
			site.AddPart(new SitePart(site, sitePartDef, SitePartDefOf.PossibleUnknownThreatMarker.Worker.GenerateDefaultParams(0f, tile, faction)));
			site.desiredThreatPoints = site.ActualThreatPoints;
			return site;
		}
	}
}
