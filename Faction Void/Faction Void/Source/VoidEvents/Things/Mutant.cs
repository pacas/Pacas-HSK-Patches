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
    public class Mutant : Pawn
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                var hediff = HediffMaker.MakeHediff(VoidDefOf.Void_RapidHealing, this);
                this.health.AddHediff(hediff);

                var painStop = HediffMaker.MakeHediff(VoidDefOf.Void_Painstopper, this);
                this.health.AddHediff(painStop);
            }
        }
    }
}