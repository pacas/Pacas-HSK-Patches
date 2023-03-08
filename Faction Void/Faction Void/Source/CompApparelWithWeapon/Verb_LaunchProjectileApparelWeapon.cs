using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VoidEvents
{
	public class Verb_LaunchProjectileApparelWeapon : Verb
	{
		public virtual ThingDef Projectile
		{
			get
			{
				if (base.EquipmentSource != null)
				{
					CompChangeableProjectile comp = base.EquipmentSource.GetComp<CompChangeableProjectile>();
					if (comp != null && comp.Loaded)
					{
						return comp.Projectile;
					}
				}
				return verbProps.defaultProjectile;
			}
		}

		public override void WarmupComplete()
		{
			base.WarmupComplete();
			Find.BattleLog.Add(new BattleLogEntry_RangedFire(caster, currentTarget.HasThing ? currentTarget.Thing : null, base.EquipmentSource?.def, Projectile, ShotsPerBurst > 1));
		}

		public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
		{
			if (caster == null)
			{
				Log.Error("Verb " + GetUniqueLoadID() + " needs caster to work (possibly lost during saving/loading).");
				return false;
			}
			if (!caster.Spawned)
			{
				return false;
			}
			if (state == VerbState.Bursting || !CanHitTarget(castTarg))
			{
				return false;
			}
			this.surpriseAttack = surpriseAttack;
			canHitNonTargetPawnsNow = canHitNonTargetPawns;
			currentTarget = castTarg;
			currentDestination = destTarg;
			WarmupComplete();
			return true;
		}
		protected override bool TryCastShot()
		{
			if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
			{
				return false;
			}
			ThingDef projectile = Projectile;
			if (projectile == null)
			{
				return false;
			}
			bool flag = TryFindShootLineFromTo(caster.Position, currentTarget, out ShootLine resultingLine);
			if (verbProps.stopBurstWithoutLos && !flag)
			{
				return false;
			}
			if (base.EquipmentSource != null)
			{
				base.EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
				base.EquipmentSource.GetComp<CompReloadable>()?.UsedOnce();
			}
			Thing launcher = caster;
			Thing equipment = base.EquipmentSource;
			CompMannable compMannable = caster.TryGetComp<CompMannable>();
			if (compMannable != null && compMannable.ManningPawn != null)
			{
				launcher = compMannable.ManningPawn;
				equipment = caster;
			}
			Vector3 drawPos = caster.DrawPos;
			Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, resultingLine.Source, caster.Map);
			if (verbProps.ForcedMissRadius > 0.5f)
			{
				float num = VerbUtility.CalculateAdjustedForcedMiss(verbProps.ForcedMissRadius, currentTarget.Cell - caster.Position);
				if (num > 0.5f)
				{
					int max = GenRadial.NumCellsInRadius(num);
					int num2 = Rand.Range(0, max);
					if (num2 > 0)
					{
						IntVec3 c = currentTarget.Cell + GenRadial.RadialPattern[num2];
						ThrowDebugText("ToRadius");
						ThrowDebugText("Rad\nDest", c);
						ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
						if (Rand.Chance(0.5f))
						{
							projectileHitFlags = ProjectileHitFlags.All;
						}
						if (!canHitNonTargetPawnsNow)
						{
							projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
						}
						projectile2.Launch(launcher, c, currentTarget, projectileHitFlags, false, equipment);
						return true;
					}
				}
			}
			ShotReport shotReport = ShotReport.HitReportFor(caster, this, currentTarget);
			Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
			ThingDef targetCoverDef = randomCoverToMissInto?.def;
			if (!Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
			{
				resultingLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget);
				ThrowDebugText("ToWild" + (canHitNonTargetPawnsNow ? "\nchntp" : ""));
				ThrowDebugText("Wild\nDest", resultingLine.Dest);
				ProjectileHitFlags projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
				if (Rand.Chance(0.5f) && canHitNonTargetPawnsNow)
				{
					projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
				}
				projectile2.Launch(launcher, drawPos, resultingLine.Dest, currentTarget, projectileHitFlags2, false, equipment, targetCoverDef);
				return true;
			}
			if (currentTarget.Thing != null && currentTarget.Thing.def.category == ThingCategory.Pawn && !Rand.Chance(shotReport.PassCoverChance))
			{
				ThrowDebugText("ToCover" + (canHitNonTargetPawnsNow ? "\nchntp" : ""));
				ThrowDebugText("Cover\nDest", randomCoverToMissInto.Position);
				ProjectileHitFlags projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
				if (canHitNonTargetPawnsNow)
				{
					projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
				}
				projectile2.Launch(launcher, drawPos, randomCoverToMissInto, currentTarget, projectileHitFlags3, false, equipment, targetCoverDef);
				return true;
			}
			ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
			if (canHitNonTargetPawnsNow)
			{
				projectileHitFlags4 |= ProjectileHitFlags.NonTargetPawns;
			}
			if (!currentTarget.HasThing || currentTarget.Thing.def.Fillage == FillCategory.Full)
			{
				projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
			}
			ThrowDebugText("ToHit" + (canHitNonTargetPawnsNow ? "\nchntp" : ""));
			if (currentTarget.Thing != null)
			{
				projectile2.Launch(launcher, drawPos, currentTarget, currentTarget, projectileHitFlags4, false, equipment, targetCoverDef);
				ThrowDebugText("Hit\nDest", currentTarget.Cell);
			}
			else
			{
				projectile2.Launch(launcher, drawPos, resultingLine.Dest, currentTarget, projectileHitFlags4, false, equipment, targetCoverDef);
				ThrowDebugText("Hit\nDest", resultingLine.Dest);
			}
			return true;
		}

		public void TryCastNextBurstShotCustom()
		{
			LocalTargetInfo localTargetInfo = currentTarget;
			if (Available() && TryCastShot())
			{
				if (verbProps.muzzleFlashScale > 0.01f)
				{
					FleckMaker.Static(caster.Position, caster.Map, FleckDefOf.ShotFlash, verbProps.muzzleFlashScale);
				}
				if (verbProps.soundCast != null)
				{
					verbProps.soundCast.PlayOneShot(new TargetInfo(caster.Position, caster.Map));
				}
				if (verbProps.soundCastTail != null)
				{
					verbProps.soundCastTail.PlayOneShotOnCamera(caster.Map);
				}
				if (CasterIsPawn)
				{
					if (CasterPawn.thinker != null)
					{
						CasterPawn.mindState.lastEngageTargetTick = Find.TickManager.TicksGame;

					}
					if (CasterPawn.mindState != null)
					{
						CasterPawn.mindState.lastAttackTargetTick = Find.TickManager.TicksGame;
						CasterPawn.mindState.lastAttackedTarget = localTargetInfo;
					}
					if (CasterPawn.MentalState != null)
					{
						CasterPawn.MentalState.Notify_AttackedTarget(localTargetInfo);
					}
					if (TerrainDefSource != null)
					{
						CasterPawn.meleeVerbs.Notify_UsedTerrainBasedVerb();
					}
					if (CasterPawn.health != null)
					{
						CasterPawn.health.Notify_UsedVerb(this, localTargetInfo);
					}
					if (EquipmentSource != null)
					{
						EquipmentSource.Notify_UsedWeapon(CasterPawn);
					}
					if (!CasterPawn.Spawned)
					{
						Reset();
						return;
					}
				}
				if (verbProps.consumeFuelPerShot > 0f)
				{
					caster.TryGetComp<CompRefuelable>()?.ConsumeFuel(verbProps.consumeFuelPerShot);
				}
				burstShotsLeft--;
			}
			else
			{
				burstShotsLeft = 0;
			}
			if (burstShotsLeft > 0)
			{
				ticksToNextBurstShot = verbProps.ticksBetweenBurstShots;
				return;
			}
			state = VerbState.Idle;
			castCompleteCallback?.Invoke();
		}

		private void ThrowDebugText(string text)
		{
			if (DebugViewSettings.drawShooting)
			{
				MoteMaker.ThrowText(caster.DrawPos, caster.Map, text);
			}
		}

		private void ThrowDebugText(string text, IntVec3 c)
		{
			if (DebugViewSettings.drawShooting)
			{
				MoteMaker.ThrowText(c.ToVector3Shifted(), caster.Map, text);
			}
		}

		public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
		{
			needLOSToCenter = true;
			return Projectile?.projectile.explosionRadius ?? 0f;
		}

		public override bool Available()
		{
			if (!base.Available())
			{
				return false;
			}
			if (CasterIsPawn)
			{
				Pawn casterPawn = CasterPawn;
				if (casterPawn.Faction != Faction.OfPlayer && casterPawn.mindState.MeleeThreatStillThreat && casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
				{
					return false;
				}
			}
			return Projectile != null;
		}
	}
}