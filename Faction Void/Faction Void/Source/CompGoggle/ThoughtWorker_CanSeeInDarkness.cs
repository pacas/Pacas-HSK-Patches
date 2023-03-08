using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI.Group;
using Verse.Sound;
using Verse.AI;

namespace NightVisionGoggle
{
	public class ThoughtWorker_CanSeeInDarkness : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return p.Awake() && p.needs.mood.recentMemory.TicksSinceLastLight > 240 && HasNightVisionEnabled(p);
		}

		private bool HasNightVisionEnabled(Pawn pawn)
        {
			var apparels = pawn.apparel.WornApparel;
			if (apparels != null)
            {
				foreach (var app in apparels)
                {
					var comp = app.TryGetComp<CompGoggle>();
					if (comp?.goggleIsOn ?? false)
                    {
						return true;
                    }
                }
            }
			return false;
        }
	}
}
