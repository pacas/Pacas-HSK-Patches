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

namespace NightVisionGoggle
{

	[HarmonyPatch(typeof(StatWorker), "StatOffsetFromGear")]
    class StatOffsetFromGear_Patch
	{
        public static void Postfix(ref float __result, Thing gear, StatDef stat)
		{
			if (stat == StatDefOf.ShootingAccuracyPawn && CompGoggle.goggleThings.Contains(gear))
            {
				var comp = gear.TryGetComp<CompGoggle>();
				if (comp.goggleIsOn)
                {
					if (comp.NightVisionWorks())
                    {
						__result += 10f;
                    }
					else
                    {
						__result -= 10f;
					}
				}
			}
		}
	}
}