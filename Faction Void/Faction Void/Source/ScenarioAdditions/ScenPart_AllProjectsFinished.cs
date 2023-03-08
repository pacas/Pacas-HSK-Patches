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
	public class ScenPart_UnlockAllResearch : ScenPart
	{
        public override void GenerateIntoMap(Map map)
        {
            base.GenerateIntoMap(map);
            Find.ResearchManager.DebugSetAllProjectsFinished();
        }
    }
}
