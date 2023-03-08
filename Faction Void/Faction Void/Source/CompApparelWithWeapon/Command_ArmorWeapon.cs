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
    public class Command_ArmorWeapon : Command_VerbTarget
    {
        public Command_ArmorWeapon()
        {
        }

		public Action<LocalTargetInfo, Verb> action;
		protected TargetingParameters GetTargetingParameters()
		{
			return new TargetingParameters
			{
				canTargetAnimals = verb.targetParams.canTargetAnimals,
				canTargetBuildings = verb.targetParams.canTargetBuildings,
				canTargetHumans = verb.targetParams.canTargetHumans,
				canTargetItems = verb.targetParams.canTargetItems,
				canTargetLocations = verb.targetParams.canTargetLocations,
				canTargetMechs = verb.targetParams.canTargetMechs,
				canTargetPawns = verb.targetParams.canTargetPawns,
				canTargetFires = verb.targetParams.canTargetFires,
				canTargetSelf = verb.targetParams.canTargetSelf,
				validator = ((TargetInfo x) => DrawRadius(x))
			};
		}

		public bool DrawRadius(TargetInfo x)
        {
			comp.AttackVerb.verbProps.DrawRadiusRing(pawn.Position);
			return comp.AttackVerb.CanHitTarget(x.Thing) && (x.Thing is Pawn victim && !victim.Downed || x.Thing == null || !(x.Thing is Pawn));
        }
        public override void ProcessInput(Event ev)
        {
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			Targeter targeter = Find.Targeter;
			if (verb.CasterIsPawn && targeter.targetingSource != null && targeter.targetingSource.GetVerb.verbProps == verb.verbProps)
			{
				Pawn casterPawn = verb.CasterPawn;
				if (!targeter.IsPawnTargeting(casterPawn))
				{
					targeter.targetingSourceAdditionalPawns.Add(casterPawn);
				}
			}
			else
			{
				Find.Targeter.BeginTargeting(GetTargetingParameters(), delegate (LocalTargetInfo target)
				{
					action(target, verb);
				});
			}
		}

		public Pawn pawn;
		public CompApparelWithWeapon comp;
	}
}