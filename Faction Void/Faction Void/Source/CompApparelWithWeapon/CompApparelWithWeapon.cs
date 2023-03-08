using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VoidEvents
{
    public class CompProperties_ApparelWithWeapon : CompProperties
    {
        public CompProperties_ApparelWithWeapon()
        {
            compClass = typeof(CompApparelWithWeapon);
        }

        public string label;
        public string desc;
        public string uiIcon;
        public bool autoAiming;
        public bool autoFire;
        public bool holdFire;
        public bool holdFireDefault;
        public int warmupTime = 5;
        public int cooldownTime = 2;
        public bool canTargetPawns;
        public bool canTargetBuildings;
    }
    public class CompApparelWithWeapon : ThingComp, IVerbOwner
    {
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetWornGizmosExtra())
            {
                yield return item;
            }
            foreach (Gizmo item in GetVerbsCommands())
            {
                yield return item;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                holdFire = Props.holdFireDefault;
                burstCooldownTicksLeft = AttackVerb != null ? BurstCooldownTime().SecondsToTicks() : 999;
            }
            UpdateVerbs();
        }

        private void UpdateVerbs()
        {
            if (Wearer != null)
            {
                foreach (Verb verb in AllVerbs)
                {
                    if (verb.caster != Wearer)
                    {
                        verb.caster = Wearer;
                        verb.castCompleteCallback = BurstComplete;
                    }
                }
            }
        }
        private VerbTracker verbTracker;
        public CompProperties_ApparelWithWeapon Props => props as CompProperties_ApparelWithWeapon;
        public Apparel Apparel => parent as Apparel;
        public Pawn Wearer => Apparel.Wearer;
        public List<VerbProperties> VerbProperties => parent.def.Verbs;

        public List<Tool> Tools => parent.def.tools;

        public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;

        public Thing ConstantCaster => Wearer;

        public VerbTracker VerbTracker
        {
            get
            {
                if (verbTracker == null)
                {
                    verbTracker = new VerbTracker(this);
                }
                return verbTracker;
            }
        }

        public List<Verb> AllVerbs => VerbTracker.AllVerbs;

        public string UniqueVerbOwnerID()
        {
            return "ApparelWithWeapon_" + parent.ThingID;
        }

        public bool VerbsStillUsableBy(Pawn p)
        {
            return Wearer == p;
        }

        public IEnumerable<Command> GetVerbsCommands(KeyCode hotKey = KeyCode.None)
        {
            UpdateVerbs();
            List<Verb> verbs = AllVerbs;
            for (int i = 0; i < verbs.Count; i++)
            {
                Verb verb = verbs[i];
                if (verb.verbProps.hasStandardCommand)
                {
                    yield return CreateVerbTargetCommand(Apparel, verb);
                }
            }

            if (Wearer?.Faction == Faction.OfPlayer && CanToggleHoldFire)
            {
                Command_Toggle command_Toggle = new Command_Toggle
                {
                    defaultLabel = "CommandHoldFire".Translate(),
                    defaultDesc = "CommandHoldFireDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire"),
                    hotKey = KeyBindingDefOf.Misc6,
                    toggleAction = delegate
                    {
                        holdFire = !holdFire;
                        if (holdFire)
                        {
                            ResetCurrentTarget();
                        }
                    },
                    isActive = () => holdFire
                };
                yield return command_Toggle;
            }
        }

        public Verb AttackVerb { get; private set; }
        public LocalTargetInfo CurrentTarget => currentTargetInt;

        protected int burstCooldownTicksLeft;

        protected int burstWarmupTicksLeft;
        public override void CompTick()
        {
            base.CompTick();
            if (!CanToggleHoldFire)
            {
                holdFire = false;
            }

            if (currentTargetInt.ThingDestroyed)
            {
                ResetCurrentTarget();
            }
            if (AttackVerb != null)
            {
                if (!CurrentTarget.IsValid)
                {
                    currentTargetInt = TryFindNewTarget();
                }
                if (CurrentTarget.IsValid)
                {
                    verbTracker.VerbsTick();
                    if (AttackVerb.state == VerbState.Bursting)
                    {
                        return;
                    }
                    if (WarmingUp)
                    {
                        burstWarmupTicksLeft--;
                        if (burstWarmupTicksLeft == 0)
                        {
                            BeginBurst();
                        }
                    }
                    else
                    {
                        if (burstCooldownTicksLeft > 0)
                        {
                            burstCooldownTicksLeft--;
                        }
                        if (burstCooldownTicksLeft <= 0 && parent.IsHashIntervalTick(10))
                        {
                            TryStartShootSomething(canBeginBurstImmediately: true);
                        }
                    }
                }

            }
            else if (Props.autoFire)
            {
                AttackVerb = AllVerbs.First();
                TryStartShootSomething(canBeginBurstImmediately: true);
            }
            else
            {
                ResetCurrentTarget();
            }
        }

        protected void BeginBurst()
        {
            if (holdFire && CanToggleHoldFire)
            {
                return;
            }
            if (AttackVerb is null)
            {
                return;
            }
            if (AttackVerb.caster != Wearer)
            {
                AttackVerb.caster = Wearer;
            }
            if (AttackVerb.caster is null)
            {
                return;
            }
            if (CurrentTarget.Thing is Pawn pawn && (pawn.Downed || pawn.Dead))
            {
                ResetCurrentTarget();
            }
            else if (AttackVerb.TryStartCastOn(CurrentTarget))
            {
                burstCooldownTicksLeft = BurstCooldownTime().SecondsToTicks();
            }
            else
            {
                ResetCurrentTarget();
            }
            if (!Props.autoFire)
            {
                AttackVerb = null;
                ResetCurrentTarget();
            }
        }

        private Command_VerbTarget CreateVerbTargetCommand(Thing ownerThing, Verb verb)
        {
            Command_ArmorWeapon command_VerbTarget = new Command_ArmorWeapon
            {
                defaultLabel = Props.label,
                defaultDesc = Props.desc,
                icon = ContentFinder<Texture2D>.Get(Props.uiIcon),
                tutorTag = "VerbTarget",
                verb = verb,
                pawn = Wearer,
                comp = this,
                action = delegate (LocalTargetInfo t, Verb verb2)
                {
                    currentTargetInt = t;
                    AttackVerb = verb2;
                }
            };
            if (verb.caster.Faction != Faction.OfPlayer)
            {
                command_VerbTarget.Disable("CannotOrderNonControlled".Translate());
            }
            else if (verb.CasterIsPawn)
            {
                if (verb.CasterPawn.WorkTagIsDisabled(WorkTags.Violent))
                {
                    command_VerbTarget.Disable("IsIncapableOfViolence".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
                }
                else if (!verb.CasterPawn.drafter.Drafted)
                {
                    command_VerbTarget.Disable("IsNotDrafted".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
                }
            }
            return command_VerbTarget;
        }

        public Verb GetVerb(VerbCategory category)
        {
            List<Verb> allVerbs = AllVerbs;
            if (allVerbs != null)
            {
                for (int i = 0; i < allVerbs.Count; i++)
                {
                    if (allVerbs[i].verbProps.category == category)
                    {
                        return allVerbs[i];
                    }
                }
            }
            return null;
        }

        protected LocalTargetInfo currentTargetInt = LocalTargetInfo.Invalid;

        private bool holdFire;
        private bool WarmingUp => burstWarmupTicksLeft > 0;
        private bool CanToggleHoldFire => Props.holdFire;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref burstCooldownTicksLeft, "burstCooldownTicksLeft", 0);
            Scribe_Values.Look(ref burstWarmupTicksLeft, "burstWarmupTicksLeft", 0);
            Scribe_TargetInfo.Look(ref currentTargetInt, "currentTarget");
            Scribe_Values.Look(ref holdFire, "holdFire", defaultValue: false);
        }

        protected void TryStartShootSomething(bool canBeginBurstImmediately)
        {
            if (!Apparel.Wearer?.Spawned ?? (false || (holdFire && CanToggleHoldFire) || !AttackVerb.Available() || !(AttackVerb != null && Wearer != null)))
            {
                ResetCurrentTarget();
                return;
            }
            if (!currentTargetInt.IsValid)
            {
                currentTargetInt = TryFindNewTarget();
                burstWarmupTicksLeft = AttackVerb.verbProps.warmupTime > 0f ? AttackVerb.verbProps.warmupTime.SecondsToTicks() : 1;
            }
            else if (canBeginBurstImmediately)
            {
                BeginBurst();
            }
            else
            {
                ResetCurrentTarget();
            }
        }


        protected LocalTargetInfo TryFindNewTarget()
        {
            if (Wearer != null)
            {
                IAttackTargetSearcher attackTargetSearcher = Wearer;
                Faction faction = attackTargetSearcher.Thing.Faction;
                float range = AttackVerb.verbProps.range;
                if (Rand.Value < 0.5f && AttackVerb.ProjectileFliesOverhead() && faction.HostileTo(Faction.OfPlayer) && Wearer.Map.listerBuildings.allBuildingsColonist.Where(delegate (Building x)
                {
                    float num = AttackVerb.verbProps.EffectiveMinRange(x, Wearer);
                    float num2 = x.Position.DistanceToSquared(Wearer.Position);
                    return num2 > num * num && num2 < range * range;
                }).TryRandomElement(out Building result))
                {
                    return result;
                }

                TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
                if (!AttackVerb.ProjectileFliesOverhead())
                {
                    targetScanFlags |= TargetScanFlags.NeedLOSToAll;
                    targetScanFlags |= TargetScanFlags.LOSBlockableByGas;
                }
                if (AttackVerb.IsIncendiary_Ranged())
                {
                    targetScanFlags |= TargetScanFlags.NeedNonBurning;
                }
                return (Thing)AttackTargetFinder.BestAttackTarget(attackTargetSearcher, targetScanFlags, IsValidTarget, Mathf.Max(0f, AttackVerb.verbProps.minRange),
                    Mathf.Min(9999f, AttackVerb.verbProps.range), default, float.MaxValue, canTakeTargetsCloserThanEffectiveMinRange: false);
            }
            return LocalTargetInfo.Invalid;
        }

        private bool IsValidTarget(Thing t)
        {
            if (t is Pawn pawn)
            {
                if (AttackVerb.ProjectileFliesOverhead())
                {
                    RoofDef roofDef = Wearer.Map.roofGrid.RoofAt(t.Position);
                    if (roofDef != null && roofDef.isThickRoof)
                    {
                        return false;
                    }
                }
                return !GenAI.MachinesLike(Wearer.Faction, pawn);
            }
            return true;
        }
        protected void BurstComplete()
        {
            burstCooldownTicksLeft = BurstCooldownTime().SecondsToTicks();
        }

        protected float BurstCooldownTime()
        {
            return AttackVerb.verbProps.defaultCooldownTime;
        }
        private void ResetCurrentTarget()
        {
            currentTargetInt = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = AttackVerb.verbProps.warmupTime.SecondsToTicks();
            burstCooldownTicksLeft = BurstCooldownTime().SecondsToTicks();
        }
    }
}