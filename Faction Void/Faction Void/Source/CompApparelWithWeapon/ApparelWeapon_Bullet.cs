using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VoidEvents
{
    public class ApparelWeapon_Bullet : Projectile
    {
        private Apparel ApparelWeapon(Thing launcher)
        {
            if (launcher is Pawn pawn && pawn.apparel?.WornApparel != null)
            {
                foreach (Apparel ap in pawn.apparel.WornApparel)
                {
                    if (ap.TryGetComp<CompApparelWithWeapon>() != null)
                    {
                        return ap;
                    }
                }
            }
            return null;
        }

        [Obsolete]
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = base.Map;
            _ = base.Position;
            base.Impact(hitThing);
            //Log.Message(launcher + " - " + hitThing + " - " + intendedTarget.Thing + " - " + ApparelWeapon(launcher)?.def + " - " + def + " - " + targetCoverDef);
            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact
                (launcher, hitThing, intendedTarget.Thing, ApparelWeapon(launcher)?.def, def, targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            if (hitThing != null)
            {
                DamageInfo dinfo = new DamageInfo(def.projectile.damageDef, base.DamageAmount, base.ArmorPenetration, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
                hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
                if (hitThing is Pawn pawn && pawn.stances != null && pawn.BodySize <= def.projectile.StoppingPower + 0.001f)
                {
                    pawn.stances.StaggerFor(95);
                }
                if (def.projectile.extraDamages != null)
                {
                    foreach (ExtraDamage extraDamage in def.projectile.extraDamages)
                    {
                        if (Rand.Chance(extraDamage.chance))
                        {
                            DamageInfo dinfo2 = new DamageInfo(extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(), ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
                            hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);
                        }
                    }
                }
            }
            else
            {
                SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(base.Position, map));
                if (base.Position.GetTerrain(map).takeSplashes)
                {
                    FleckMaker.WaterSplash(ExactPosition, map, Mathf.Sqrt(base.DamageAmount) * 1f, 4f);
                }
                else
                {
                    FleckMaker.Static(ExactPosition, map, FleckDefOf.ShotHit_Dirt);
                }
            }
        }
    }
}