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
	public class PlanetaryKiller : Building
	{
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            var gameCond = Find.World.GameConditionManager.GetActiveCondition(VoidDefOf2.Void_PlanetKiller);
            if (gameCond is GameCondition_VoidPlanetkiller planetKiller)
            {
                planetKiller.EndNoImpact();

            }

            var comp = this.Map.Parent.GetComponent<VoidReinforcements>();
            if (comp != null)
            {
                comp.planetKillerIsDestroyed = true;
            }
            base.Destroy(mode);
        }
    }
}
