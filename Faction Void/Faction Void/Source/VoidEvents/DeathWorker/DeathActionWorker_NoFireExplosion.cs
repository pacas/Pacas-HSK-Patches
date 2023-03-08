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
using Verse.Sound;

namespace VoidEvents
{
	public class DeathActionWorker_NoFireExplosion : DeathActionWorker
	{
		public override RulePackDef DeathRules => RulePackDefOf.Transition_DiedExplosive;
		public override bool DangerousInMelee => true;
		public override void PawnDied(Corpse corpse)
		{
			GenExplosion.DoExplosion(radius: 1f, center: corpse.Position, map: corpse.Map, damType: DamageDefOf.Bomb, instigator: corpse.InnerPawn, chanceToStartFire: 0);
		}
	}
}