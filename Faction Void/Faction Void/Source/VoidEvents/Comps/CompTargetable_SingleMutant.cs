using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;

namespace VoidEvents
{
	public class MutantsToTarget : DefModExtension
	{
		public List<PawnKindDef> mutants;
	}
	public class CompTargetable_SingleMutant : CompTargetable
	{
        public override bool PlayerChoosesTarget => true;
		public override TargetingParameters GetTargetingParameters()
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				canTargetBuildings = false,
				validator = ((TargetInfo x) => BaseTargetValidator(x.Thing) && CanTargetPawn(x.Thing as Pawn))
			};
		}

        public override void DoEffect(Pawn usedBy)
        {

        }

        public bool CanTargetPawn(Pawn pawn)
        {
			if (pawn != null)
            {
				var options = this.parent.def.GetModExtension<MutantsToTarget>();
				if (options != null && options.mutants.Contains(pawn.kindDef))
				{
					return true;
				}
			}
			return false;
        }

		public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
		{
			yield return targetChosenByPlayer;
		}
	}
}
