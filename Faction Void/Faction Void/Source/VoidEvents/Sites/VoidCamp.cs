using RimWorld.BaseGen;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoidEvents
{
    public class VoidCamp : Site
	{
        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            var comp = this.GetComponent<VoidReinforcements>();
            if (comp.planetKillerIsDestroyed)
            {
                return base.ShouldRemoveMapNow(out alsoRemoveWorldObject);
            }
            else
            {
                alsoRemoveWorldObject = false;
                return false;
            }
        }
    }
}
