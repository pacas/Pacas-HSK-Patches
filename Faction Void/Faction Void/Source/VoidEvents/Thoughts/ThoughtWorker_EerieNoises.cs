using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;

namespace VoidEvents
{
	public class ThoughtWorker_EerieNoises : ThoughtWorker
	{
        public override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.Map != null && p.Map.listerThings.ThingsOfDef(VoidDefOf.Void_DefoliatorShipPart).Any())
			{
				return ThoughtState.ActiveDefault;
			}
			return ThoughtState.Inactive;
		}
	}
}
