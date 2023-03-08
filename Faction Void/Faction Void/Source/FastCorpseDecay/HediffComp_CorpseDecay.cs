using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ChickenCorpses
{    
    class CompApplyHediff : ThingComp
    {
        public CompProperties_ApplyHediff Props => (CompProperties_ApplyHediff)base.props;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (base.parent is Pawn pawn)
            {
                if (!(pawn.health?.hediffSet?.HasHediff(Props.hediff, false) ?? true))
                {
                    Hediff hediff = HediffMaker.MakeHediff(Props.hediff, pawn, null);
                    pawn.health.AddHediff(hediff);
                }
                RemoveFromParent();
            }
            else Log.Error("[ChickenCorpses] CompApplyHediff initialized on non-pawn Thing!");
        }
        public void RemoveFromParent()
        {
            base.parent.AllComps.Remove(this);
        }
    }
    class CompProperties_ApplyHediff : CompProperties
    {
#pragma warning disable CS0649
        public HediffDef hediff;
#pragma warning restore CS0649
        public CompProperties_ApplyHediff()
        {
            base.compClass = typeof(CompApplyHediff);
        }
    }
    class HediffComp_AddThingCompOnDeath : HediffComp
    {
        HediffCompProperties_AddThingCompOnDeath Props => (HediffCompProperties_AddThingCompOnDeath)base.props;
        public override void Notify_PawnDied()
        {
            base.Notify_PawnDied();
            Pawn pawn = base.parent.pawn;
            Corpse corpse;
            if ((corpse = pawn.Corpse) != null)
            {
                ThingComp comp = (ThingComp)Activator.CreateInstance(Props.comp.compClass);
                comp.parent = corpse;
                corpse.AllComps.Add(comp);
                comp.Initialize(Props.comp);
            }
            else Log.Error("[ChickenCorpses] HediffComp_CorpseDecay: Notify_PawnDied() called but corpse is null!");
        }
    }
    class HediffCompProperties_AddThingCompOnDeath : HediffCompProperties
    {
#pragma warning disable CS0649
        public CompProperties comp;
#pragma warning restore CS0649
        public HediffCompProperties_AddThingCompOnDeath()
        {
            base.compClass = typeof(HediffComp_AddThingCompOnDeath);
        }
    }
    class CompDecayAfterDelay : ThingComp
    {
        public CompProperties_DecayAfterDelay Props => (CompProperties_DecayAfterDelay)base.props;
        int ticksToDestroy;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            ticksToDestroy = Props.delayTicks.RandomInRange;
        }
        public override void CompTick()
        {
            ticksToDestroy--;
            if (ticksToDestroy <= 0) Destroy();
        }
        public override void CompTickRare()
        {
            ticksToDestroy -= 250;
            if (ticksToDestroy <= 0) Destroy();
        }
        public void Destroy()
        {
            MakeMotes();
            base.parent.Destroy();            
        }
        public void MakeMotes()
        {
            if(base.parent.Position.ShouldSpawnMotesAt(base.parent.Map) && !base.parent.Map.moteCounter.SaturatedLowPriority)
            {
                for (int i = 0; i < Props.moteCount; i++)
                {
                    MoteThrown mote = (MoteThrown)ThingMaker.MakeThing(Props.moteDef, null);
                    mote.Scale = Props.scale;
                    mote.rotationRate = Props.rotationRate.RandomInRange;
                    mote.exactPosition = base.parent.Position.ToVector3();
                    mote.SetVelocity(Rand.Range(0, 360), Props.velocityRange.RandomInRange);
                    GenSpawn.Spawn(mote, base.parent.Position, base.parent.Map);
                }
            }            
        }
    }
    class CompProperties_DecayAfterDelay : CompProperties
    {
#pragma warning disable CS0649
        public IntRange delayTicks;
        public ThingDef moteDef;
        public int moteCount = 1;
        public float scale = 1.9f;
        public FloatRange rotationRate = new FloatRange(-60f, 60f), 
                          velocityRange = new FloatRange(0.6f, 0.75f);
#pragma warning restore CS0649
        public CompProperties_DecayAfterDelay()
        {
            base.compClass = typeof(CompDecayAfterDelay);
        }
    }
}
