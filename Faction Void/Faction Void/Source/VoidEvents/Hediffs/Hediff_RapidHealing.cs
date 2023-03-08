using RimWorld;
using Verse;

namespace VoidEvents
{
    public class Hediff_RapidHealing : Hediff_Regen
    {
        public int lastHarmTick;
        public override bool ShouldRemove => false;
        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            lastHarmTick = Find.TickManager.TicksGame;
        }
        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame < lastHarmTick + (4 * GenDate.TicksPerHour))
            {
                Severity = 0;
            }
            else
            {
                Severity = 1f;
            }

            if (Severity == 1f)
            {
                if (Find.TickManager.TicksGame % 240 == 0)
                {
                    var comp = Current.Game.GetComponent<VoidGameComp>();
                    comp.hediff_Regenerations.Add(this);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastHarmTick, "lastHarmTick");
        }
    }
}