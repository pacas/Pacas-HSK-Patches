using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ScenarioAdditions
{
	public class ScenPart_CallForReinforcements : ScenPart
	{
        public FactionDef factionToCall;
        public string gizmoIconTexPath;
        public string gizmoLabel;
        public string gizmoDesc;
        public int cooldownTicks = -1;
        public FactionRelations factionRequirement;

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref factionToCall, "factionToCall");
            Scribe_Values.Look(ref gizmoIconTexPath, "gizmoIconTexPath");
            Scribe_Values.Look(ref gizmoLabel, "gizmoLabel");
            Scribe_Values.Look(ref gizmoDesc, "gizmoDesc");
            Scribe_Values.Look(ref cooldownTicks, "cooldownTicks");
            Scribe_Deep.Look(ref factionRequirement, "factionRequirement");
        }
    }
}
