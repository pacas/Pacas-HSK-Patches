using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VoidEvents
{
    public class JobDriver_SwallowItem : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => TargetA.ThingDestroyed || TargetA.Thing.Spawned is false || TargetA.Thing is Pawn pawn && pawn.Downed is false && pawn.Dead is false);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return WithProgressBar(Toils_General.Wait(180, TargetIndex.A), TargetIndex.A, () => 1f - (pawn.jobs.curDriver.ticksLeftThisToil / (float)180), true, -0.5f, true);
            yield return Toils_General.Do(delegate
            {
                var comp = pawn.GetComp<CompSwallowedItems>();
                TargetA.Thing.DeSpawn();
                comp.innerContainer.TryAddOrTransfer(TargetA.Thing);
            });
        }

        public static Toil WithProgressBar(Toil toil, TargetIndex ind, Func<float> progressGetter, bool interpolateBetweenActorAndTarget = false, float offsetZ = -0.5f, bool alwaysShow = false)
        {
            Effecter effecter = null;
            toil.AddPreTickAction(delegate
            {
                if (effecter == null)
                {
                    var progressBar = EffecterDefOf.ProgressBar;
                    effecter = progressBar.Spawn();
                }
                else
                {
                    var target = toil.actor.CurJob.GetTarget(ind);
                    if (!target.IsValid || (target.HasThing && !target.Thing.Spawned))
                    {
                        effecter.EffectTick(toil.actor, TargetInfo.Invalid);
                    }
                    else if (interpolateBetweenActorAndTarget)
                    {
                        effecter.EffectTick(toil.actor.CurJob.GetTarget(ind).ToTargetInfo(toil.actor.Map), toil.actor);
                    }
                    else
                    {
                        effecter.EffectTick(toil.actor.CurJob.GetTarget(ind).ToTargetInfo(toil.actor.Map), TargetInfo.Invalid);
                    }
                    var mote = ((SubEffecter_ProgressBar)effecter.children[0]).mote;
                    if (mote != null)
                    {
                        mote.progress = Mathf.Clamp01(progressGetter());
                        mote.offsetZ = offsetZ;
                        mote.alwaysShow = alwaysShow;
                    }
                }
            });
            toil.AddFinishAction(delegate
            {
                if (effecter != null)
                {
                    effecter.Cleanup();
                    effecter = null;
                }
            });
            return toil;
        }
    }


}
