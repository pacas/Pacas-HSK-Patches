using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;

namespace VoidEvents
{
	public class QuestNode_RaidersArrive : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		public SlateRef<IEnumerable<Pawn>> pawns;

		public SlateRef<PawnsArrivalModeDef> arrivalMode;

		public SlateRef<IntVec3?> walkInSpot;

		public SlateRef<string> customLetterLabel;

		public SlateRef<string> customLetterText;

		public SlateRef<LetterDef> customLetterDef;

		public SlateRef<RulePack> customLetterLabelRules;

		public SlateRef<RulePack> customLetterTextRules;

		private const string RootSymbol = "root";

        public override bool TestRunInt(Slate slate)
		{
			if (!slate.Exists("map"))
			{
				return false;
			}
			return true;
		}

        public override void RunInt()
		{
			Slate slate = QuestGen.slate;
			PawnsArrivalModeDef pawnsArrivalModeDef = arrivalMode.GetValue(slate) ?? PawnsArrivalModeDefOf.EdgeWalkIn;
			QuestPart_PawnsArrive pawnsArrive = new QuestPart_PawnsArrive();
			pawnsArrive.inSignal = (QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal"));
			pawnsArrive.pawns.AddRange(pawns.GetValue(slate));
			var raiderFaction = pawnsArrive.pawns.First().Faction;
			pawnsArrive.customLetterDef = customLetterDef.GetValue(slate);
			LordMaker.MakeNewLord(raiderFaction, new LordJob_AssaultColony(raiderFaction), QuestGen.slate.Get<Map>("map"), pawnsArrive.pawns);
			pawnsArrive.arrivalMode = pawnsArrivalModeDef;
			pawnsArrive.mapParent = QuestGen.slate.Get<Map>("map").Parent;
			if (pawnsArrivalModeDef.walkIn)
			{
				pawnsArrive.spawnNear = (walkInSpot.GetValue(slate) ?? QuestGen.slate.Get<IntVec3?>("walkInSpot") ?? IntVec3.Invalid);
			}
			if (!customLetterLabel.GetValue(slate).NullOrEmpty() || customLetterLabelRules.GetValue(slate) != null)
			{
				QuestGen.AddTextRequest("root", delegate (string x)
				{
					pawnsArrive.customLetterLabel = x;
				}, QuestGenUtility.MergeRules(customLetterLabelRules.GetValue(slate), customLetterLabel.GetValue(slate), "root"));
			}
			if (!customLetterText.GetValue(slate).NullOrEmpty() || customLetterTextRules.GetValue(slate) != null)
			{
				QuestGen.AddTextRequest("root", delegate (string x)
				{
					pawnsArrive.customLetterText = x;
				}, QuestGenUtility.MergeRules(customLetterTextRules.GetValue(slate), customLetterText.GetValue(slate), "root"));
			}
			QuestGen.quest.AddPart(pawnsArrive);
		}
	}
}
