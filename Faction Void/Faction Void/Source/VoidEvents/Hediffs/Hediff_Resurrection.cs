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
    public class Hediff_Resurrection : HediffWithComps
    {
        public override void Notify_PawnDied()
        {
            base.Notify_PawnDied();
            var comp = Current.Game.GetComponent<VoidGameComp>();
            if (!comp.pawnsToResurrect.Where(x => x.pawnToResurrect == this.pawn).Any())
            {
                var options = this.def.GetModExtension<ResurrectionOptions>();
                comp.pawnsToResurrect.Add(new PawnToResurrect(this.pawn, Find.TickManager.TicksAbs + options.ticksToResurrect, this.pawn.Tile));
            }
        }
        
        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
            var comp = Current.Game.GetComponent<VoidGameComp>();
            if (!comp.pawnsToResurrect.Where(x => x.pawnToResurrect == this.pawn).Any())
            {
                var options = this.def.GetModExtension<ResurrectionOptions>();
                comp.pawnsToResurrect.Add(new PawnToResurrect(this.pawn, Find.TickManager.TicksAbs + options.ticksToResurrect, this.pawn.Tile));
            }
        }
    }
}