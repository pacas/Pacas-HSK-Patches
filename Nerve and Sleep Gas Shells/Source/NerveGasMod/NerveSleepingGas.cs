using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace NerveSleepingGas
{
    [DefOf]
    public class NerveSleepingGasDefOf
    {
        public static HediffDef NGS_NerveGas;
        public static HediffDef NGS_SleepGas;
        public static BodyPartDef Lung;
        
    }
    public class Hediff_NerveGas : HediffWithComps
    {
        public static float toxicBurnDamage = 3f;
        public static float toxicBurnDamageTickInterval = 200f;
        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % toxicBurnDamageTickInterval == 0)
            {
                foreach (var lung in this.pawn.health.hediffSet.GetNotMissingParts().Where(x => x.def == NerveSleepingGasDefOf.Lung))
                {
                    if (this.Severity > 0.1)
                    {
                        var damage = toxicBurnDamage * this.Severity;
                        var damInfo = new DamageInfo(DamageDefOf.Burn, damage, hitPart: lung);
                        this.pawn.TakeDamage(damInfo);
                    }
                }
            }
        }
    }

    public class GasWithHediff : Gas
    {
        public static float sleepDamageTickInterval = 30f;
        public static float sleepAdjustAmount = 0.05f;
        public virtual HediffDef hediffDef { get; }
        public override void Tick()
        {
            base.Tick();
            if (this.Map != null && Find.TickManager.TicksGame % sleepDamageTickInterval == 0)
            {
                foreach (var pos in GenRadial.RadialCellsAround(this.Position, 5f, true))
                {
                    if (GenGrid.InBounds(pos, this.Map))
                    {
                        var list = this.Map.thingGrid.ThingsListAt(pos);
                        for (int num = list.Count - 1; num >= 0; num--)
                        {
                            if (list[num] is Pawn pawn && !pawn.Dead && !HasGasProtectiveGears(pawn) && !pawn.RaceProps.IsMechanoid && pawn.RaceProps.IsFlesh)
                            {
                                HealthUtility.AdjustSeverity(pawn, hediffDef, sleepAdjustAmount);
                            }
                        }
                    }
                }
            }
        }

        public bool HasGasProtectiveGears(Pawn pawn)
        {
            if (pawn.apparel?.WornApparel != null)
            {
                foreach (var apparel in pawn.apparel.WornApparel)
                {
                    if (apparel.def.defName == "VAE_Headgear_GasMask" ||
                        apparel.def.defName == "Apparello_Gas" || 
                        apparel.def.defName == "Apparello_ViperGas" || 
                        apparel.def.defName == "Apparello_Meffect" || 
                        apparel.def.defName == "Apparel_DoomHelmet" || 
                        apparel.def.defName == "Apparello_Meffectwo" || 
                        apparel.def.defName == "Apparel_HellPowerArmorHelmet")
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
    public class Gas_Nerve : GasWithHediff
    {
        public override HediffDef hediffDef { get => NerveSleepingGasDefOf.NGS_NerveGas;}
    }
    public class Gas_Sleep : GasWithHediff
    {
        public override HediffDef hediffDef { get => NerveSleepingGasDefOf.NGS_SleepGas; }
    }
}
