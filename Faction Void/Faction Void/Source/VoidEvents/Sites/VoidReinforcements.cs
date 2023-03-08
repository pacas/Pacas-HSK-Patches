using EncounterFramework;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Linq;
using Verse;

namespace VoidEvents
{
    public class WorldObjectCompProperties_VoidReinforcements : WorldObjectCompProperties_TimedDetectionRaids
    {
        public IntRange reinforcementTicksInterval;
        public ThreatGenerator threatGenerator;
        public WorldObjectCompProperties_VoidReinforcements()
        {
            this.compClass = typeof(VoidReinforcements);
        }
    }
    public class VoidReinforcements : TimedDetectionRaids
    {
        public bool planetKillerIsDestroyed;
        public WorldObjectCompProperties_VoidReinforcements Props => base.props as WorldObjectCompProperties_VoidReinforcements;
        [HarmonyPatch(typeof(TimedDetectionRaids), "StartDetectionCountdown")]
        public static class TimedDetectionRaids_StartDetectionCountdown_Patch
        {
            public static void Prefix(TimedDetectionRaids __instance, ref int ticks, ref int notifyTicks)
            {
                if (__instance is VoidReinforcements comp)
                {
                    ticks = comp.Props.reinforcementTicksInterval.RandomInRange;
                }
            }
        }

        public override void CompTick()
        {
            MapParent mapParent = (MapParent)parent;
            if (!planetKillerIsDestroyed && mapParent.HasMap)
            {
                if (GenHostility.AnyHostileActiveThreatTo(mapParent.Map, this.RaidFaction))
                {
                    if (ticksLeftToSendRaid > 0)
                    {
                        ticksLeftToSendRaid--;
                        if (ticksLeftToSendRaid <= 0)
                        {
                            Props.threatGenerator.GenerateThreat(mapParent.Map);
                            ticksLeftToSendRaid = Props.reinforcementTicksInterval.RandomInRange;
                            raidsSentCount++;
                        }
                    }
                }
            }
            else
            {
                ResetCountdown();
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref planetKillerIsDestroyed, "planetKillerIsDestroyed");
        }
    }
}
