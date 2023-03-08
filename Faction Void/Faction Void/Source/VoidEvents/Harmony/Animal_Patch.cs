using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.Sound;
using static RimWorld.FoodUtility;

namespace VoidEvents
{
    [HarmonyPatch(typeof(ManhunterPackIncidentUtility), nameof(ManhunterPackIncidentUtility.CanArriveManhunter))]
    public static class ManhunterPackIncidentUtility_CanArriveManhunter_Patch
    {
        public static void Postfix(ref bool __result, PawnKindDef kind)
        {
            if (kind.defaultFactionType != null)
            {
                if (kind.defaultFactionType == VoidDefOf.RH_VOID)
                {
                    var voidFaction = Find.FactionManager.FirstFactionOfDef(VoidDefOf.RH_VOID);
                    if (voidFaction is null || voidFaction.defeated)
                    {
                        __result = false;
                    }
                }
                else if (kind.defaultFactionType == VoidDefOf.RH2_Nerotonin4_Horde)
                {
                    var hordeFaction = Find.FactionManager.FirstFactionOfDef(VoidDefOf.RH2_Nerotonin4_Horde);
                    if (hordeFaction is null || hordeFaction.defeated)
                    {
                        __result = false;
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(RaceProperties), "Animal", MethodType.Getter)]
    internal class Animal_Patch
    {
        public static void Postfix(RaceProperties __instance, ref bool __result)
        {
            if (!__result && __instance.intelligence == Intelligence.ToolUser && __instance.body.defName.StartsWith("RH_"))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(FoodUtility), "ThoughtsFromIngesting")]
    internal class ThoughtsFromIngesting_Patch
    {
        private static readonly List<ThoughtDef> ingestThoughts = new List<ThoughtDef>();
        public static bool Prefix(ref List<ThoughtFromIngesting> __result, Pawn ingester, Thing foodSource, ThingDef foodDef)
        {
            if (!ingester?.story?.traits.HasTrait(TraitDefOf.Cannibal) ?? false)
            {
                if (IsMutantSource(foodSource, foodDef))
                {
                    List<ThoughtDef> thoughts = ThoughtsFromIngestingMutant(ingester, foodSource, foodDef);
                    __result = new List<ThoughtFromIngesting>();
                    foreach (ThoughtDef thought in thoughts)
                    {
                        __result.Add(new ThoughtFromIngesting
                        {
                            thought = thought,
                            fromPrecept = null
                        });
                    }
                    return false;
                }
            }
            return true;
        }

        public static bool IsMutantSource(Thing foodSource, ThingDef foodDef)
        {
            if (foodDef.IsMeat && foodSource.def?.ingestible?.sourceDef?.thingClass == typeof(Mutant))
            {
                return true;
            }
            else if (foodSource is Corpse corpse && corpse.InnerPawn?.def?.thingClass == typeof(Mutant))
            {
                return true;
            }
            CompIngredients compIngredients = foodSource.TryGetComp<CompIngredients>();
            if (compIngredients?.ingredients != null)
            {
                foreach (ThingDef food in compIngredients.ingredients)
                {
                    if (food.IsMeat && food.ingestible?.sourceDef?.thingClass == typeof(Mutant))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static List<ThoughtDef> ThoughtsFromIngestingMutant(Pawn ingester, Thing foodSource, ThingDef foodDef)
        {
            ingestThoughts.Clear();
            if (ingester.needs == null || ingester.needs.mood == null)
            {
                return ingestThoughts;
            }
            if (!ingester.story.traits.HasTrait(TraitDefOf.Ascetic) && foodDef.ingestible.tasteThought != null)
            {
                if (foodDef.IsMeat && foodSource.def?.ingestible?.sourceDef?.thingClass == typeof(Mutant))
                {
                    ingestThoughts.Add(VoidDefOf.Void_AteMutantMeat);
                }
                else if (foodSource is Corpse corpse && corpse.InnerPawn.def.thingClass == typeof(Mutant))
                {
                    ingestThoughts.Add(VoidDefOf.Void_AteMutantCorpse);
                }
            }
            CompIngredients compIngredients = foodSource.TryGetComp<CompIngredients>();
            if (compIngredients != null)
            {
                for (int i = 0; i < compIngredients.ingredients.Count; i++)
                {
                    AddIngestThoughtsFromIngredient(compIngredients.ingredients[i], ingester, ingestThoughts);
                }
            }

            if (foodDef.ingestible.specialThoughtDirect != null)
            {
                ingestThoughts.Add(foodDef.ingestible.specialThoughtDirect);
            }
            if (foodSource.IsNotFresh())
            {
                ingestThoughts.Add(ThoughtDefOf.AteRottenFood);
            }
            if (ModsConfig.RoyaltyActive && FoodUtility.InappropriateForTitle(foodDef, ingester, allowIfStarving: false))
            {
                ingestThoughts.Add(ThoughtDefOf.AteFoodInappropriateForTitle);
            }
            return ingestThoughts;
        }

        private static void AddIngestThoughtsFromIngredient(ThingDef ingredient, Pawn ingester, List<ThoughtDef> ingestThoughts)
        {
            if (ingredient.ingestible != null)
            {
                if (ingester.RaceProps.Humanlike && ingredient.ingestible?.sourceDef?.thingClass == typeof(Mutant))
                {
                    ingestThoughts.Add(VoidDefOf.Void_AteMutantMeat);
                }
                else if (ingredient.ingestible.specialThoughtAsIngredient != null)
                {
                    ingestThoughts.Add(ingredient.ingestible.specialThoughtAsIngredient);
                }
            }
        }

        public static bool IsHumanlikeMeat(ThingDef def)
        {
            return def.ingestible.sourceDef != null && def.ingestible.sourceDef.race != null && def.ingestible.sourceDef.race.Humanlike;
        }

        public static bool IsHumanlikeMeatOrHumanlikeCorpse(Thing thing)
        {
            if (IsHumanlikeMeat(thing.def))
            {
                return true;
            }
            return thing is Corpse corpse && corpse.InnerPawn.RaceProps.Humanlike;
        }

        public static int WillIngestStackCountOf(Pawn ingester, ThingDef def, float singleFoodNutrition)
        {
            int num = Mathf.Min(def.ingestible.maxNumToIngestAtOnce, StackCountForNutrition(ingester.needs.food.NutritionWanted, singleFoodNutrition));
            if (num < 1)
            {
                num = 1;
            }
            return num;
        }

        public static float GetBodyPartNutrition(Corpse corpse, BodyPartRecord part)
        {
            return GetBodyPartNutrition(corpse.GetStatValue(StatDefOf.Nutrition), corpse.InnerPawn, part);
        }

        public static float GetBodyPartNutrition(float currentCorpseNutrition, Pawn pawn, BodyPartRecord part)
        {
            HediffSet hediffSet = pawn.health.hediffSet;
            float coverageOfNotMissingNaturalParts = hediffSet.GetCoverageOfNotMissingNaturalParts(pawn.RaceProps.body.corePart);
            if (coverageOfNotMissingNaturalParts <= 0f)
            {
                return 0f;
            }
            float num = hediffSet.GetCoverageOfNotMissingNaturalParts(part) / coverageOfNotMissingNaturalParts;
            return currentCorpseNutrition * num;
        }

        public static int StackCountForNutrition(float wantedNutrition, float singleFoodNutrition)
        {
            return wantedNutrition <= 0.0001f ? 0 : Mathf.Max(Mathf.RoundToInt(wantedNutrition / singleFoodNutrition), 1);
        }

        public static bool ShouldBeFedBySomeone(Pawn pawn)
        {
            return FeedPatientUtility.ShouldBeFed(pawn) || WardenFeedUtility.ShouldBeFed(pawn);
        }

        public static void AddFoodPoisoningHediff(Pawn pawn, Thing ingestible, FoodPoisonCause cause)
        {
            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.FoodPoisoning);
            if (firstHediffOfDef != null)
            {
                if (firstHediffOfDef.CurStageIndex != 2)
                {
                    firstHediffOfDef.Severity = HediffDefOf.FoodPoisoning.stages[2].minSeverity - 0.001f;
                }
            }
            else
            {
                pawn.health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.FoodPoisoning, pawn));
            }
            if (PawnUtility.ShouldSendNotificationAbout(pawn) && MessagesRepeatAvoider.MessageShowAllowed("MessageFoodPoisoning-" + pawn.thingIDNumber, 0.1f))
            {
                Messages.Message("MessageFoodPoisoning".Translate(pawn.LabelShort, ingestible.LabelCapNoCount, cause.ToStringHuman().CapitalizeFirst(), pawn.Named("PAWN"), ingestible.Named("FOOD")).CapitalizeFirst(), pawn, MessageTypeDefOf.NegativeEvent);
            }
        }

        [System.Obsolete]
        public static float GetFoodPoisonChanceFactor(Pawn ingester)
        {
            float num = Find.Storyteller.difficultyValues.foodPoisonChanceFactor;
            if (ingester.health != null && ingester.health.hediffSet != null)
            {
                foreach (Hediff hediff in ingester.health.hediffSet.hediffs)
                {
                    HediffStage curStage = hediff.CurStage;
                    if (curStage != null)
                    {
                        num *= curStage.foodPoisoningChanceFactor;
                    }
                }
                return num;
            }
            return num;
        }

        public static bool Starving(Pawn p)
        {
            return p.needs != null && p.needs.food != null && p.needs.food.Starving;
        }

        public static float GetNutrition(Thing foodSource, ThingDef foodDef)
        {
            if (foodSource == null || foodDef == null)
            {
                return 0f;
            }
            return foodSource.def == foodDef ? foodSource.GetStatValue(StatDefOf.Nutrition) : foodDef.GetStatValueAbstract(StatDefOf.Nutrition);
        }

        public static bool WillIngestFromInventoryNow(Pawn pawn, Thing inv)
        {
            return (inv.def.IsNutritionGivingIngestible || inv.def.IsNonMedicalDrug) && inv.IngestibleNow && pawn.WillEat(inv);
        }

        public static void IngestFromInventoryNow(Pawn pawn, Thing inv)
        {
            Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Ingest, inv);
            job.count = Mathf.Min(inv.stackCount, WillIngestStackCountOf(pawn, inv.def, inv.GetStatValue(StatDefOf.Nutrition)));
            pawn.jobs.TryTakeOrderedJob(job);
        }
    }

    [HarmonyPatch]
    public static class DubsMinimapHidePawn
    {
        public static MethodBase method;
        public static bool Prepare()
        {
            method = AccessTools.Method("DubsMintMinimap.MainTabWindow_MiniMap:DrawAllPawns");
            return method != null;
        }
        public static MethodBase TargetMethod()
        {
            return method;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var getterInfo = AccessTools.PropertyGetter(typeof(MapPawns), nameof(MapPawns.AllPawnsSpawned));
            foreach (var code in codes)
            {
                yield return code;
                if (code.Calls(getterInfo))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DubsMinimapHidePawn),
                        nameof(FilteredList)));
                }
            }
        }

        public static List<Pawn> FilteredList(List<Pawn> pawns)
        {
            return pawns.Where(x => x.kindDef != VoidDefOf.RH_DF2_Stalker).ToList();
        }
    }

    [HarmonyPatch]
    public static class ShouldShowDotPatch
    {
        public static MethodBase method;
        public static bool Prepare()
        {
            method = AccessTools.Method("CameraPlus.Tools:ShouldShowDot");
            return method != null;
        }
        public static MethodBase TargetMethod()
        {
            return method;
        }

        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (pawn.kindDef == VoidDefOf.RH_DF2_Stalker)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(Verb), "TryFindShootLineFromTo")]
    public static class Verb_TryFindShootLineFromTo_Patch
    {
        public static void Postfix(Verb __instance, ref bool __result, LocalTargetInfo targ)
        {
            if (__instance.caster is Building_Turret && targ.Pawn?.kindDef == VoidDefOf.RH_DF2_Stalker)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(Building_Door), "PawnCanOpen")]
    public static class Building_Door_Patch
    {
        public static void Postfix(ref bool __result, Pawn p)
        {
            if (p.kindDef == VoidDefOf.RH_DF2_Stalker)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Sustainer), "SustainerUpdate")]
    public static class Sustainer_SustainerUpdate_Patch
    {
        public static bool Prefix(Sustainer __instance)
        {
            Map map = Find.CurrentMap;
            if (map != null)
            {
                IEnumerable<Pawn> stalkers = map.listerThings.ThingsOfDef(VoidDefOf.RH_DF2_Stalker.race).OfType<Pawn>().Where(x => x.Dead is false);
                if (stalkers.Any())
                {
                    if (__instance.def != VoidDefOf.RH_DF2_Stalker.race.GetCompProperties<CompProperties_AmbientSound>().ambientSound)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Pawn_Kill_Patch
    {
        public static void Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (__instance.Dead)
            {
                CompSwallowedItems comp = __instance.GetComp<CompSwallowedItems>();
                if (comp != null)
                {
                    Map map = __instance.MapHeld;
                    if (map != null)
                    {
                        comp.ReleaseSwallowedItems(map);
                    }
                }
            }
        }
    }
}