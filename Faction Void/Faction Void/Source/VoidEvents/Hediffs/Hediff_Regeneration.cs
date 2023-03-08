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

namespace VoidEvents
{
	public class Hediff_Regen : HediffWithComps
    {
		public void TryHealRandomPart()
		{
			List<BodyPartRecord> list = (from x in pawn.RaceProps.body.AllParts
										 where pawn.health.hediffSet.PartIsMissing(x)
										 select x).ToList<BodyPartRecord>();
			if (list.Count == 0)
			{
				return;
			}
			BodyPartRecord bodyPartRecord;
			if (GenCollection.TryRandomElement<BodyPartRecord>(list, out bodyPartRecord))
			{
				pawn.health.RestorePart(bodyPartRecord, null, true);
			}
			for (int num = pawn.health.hediffSet.hediffs.Count - 1; num >= 0; num--)
			{
				var hediff = pawn.health.hediffSet.hediffs[num];
				var comp = hediff.TryGetComp<HediffComp_GetsPermanent>();
				if (comp != null && comp.IsPermanent)
				{
					pawn.health.hediffSet.hediffs.RemoveAt(num);
				}
			}
		}
	}

	public class Hediff_Regeneration : Hediff_Regen
	{
        public override bool ShouldRemove => false;
        public override void Tick()
        {
            base.Tick();
			if (Find.TickManager.TicksGame % 60 == 0)
            {
				var comp = Current.Game.GetComponent<VoidGameComp>();
				comp.hediff_Regenerations.Add(this);
			}
        }


	}
}