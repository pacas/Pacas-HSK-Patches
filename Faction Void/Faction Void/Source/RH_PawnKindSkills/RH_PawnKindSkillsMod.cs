using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RH_PawnKindSkills
{
    public class PawnKindExtension : DefModExtension
    {
        public List<SkillRange> skillGains;
        public List<SkillRange> forcedSkills;
        public WorkTags forcedWorkTags;
    }

    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            new Harmony("RH_PawnKindSkills").PatchAll();
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "TryGenerateNewPawnInternal")]
    public static class PawnGenerator_TryGenerateNewPawnInternal_Patch
    {
        public static void Postfix(Pawn __result)
        {
            if (__result != null)
            {
                var extension = __result.kindDef.GetModExtension<PawnKindExtension>();
                if (extension != null)
                {
                    var requiredWorkTags = extension.forcedWorkTags;
                    var pawn = __result;
                    if (pawn.story != null && pawn.story.Childhood != null && (pawn.story.Childhood.workDisables & requiredWorkTags) != 0)
                    {
                        var backstoryCategoryFiltersFor = PawnBioAndNameGenerator.GetBackstoryCategoryFiltersFor(pawn, pawn.Faction?.def);
                        var source = DefDatabase<BackstoryDef>.AllDefs.Where((BackstoryDef bs) => bs.shuffleable 
                            && bs.slot == BackstorySlot.Childhood && (bs.workDisables & requiredWorkTags) == 0 && backstoryCategoryFiltersFor.Any(x => x.Matches(bs)));
                        if (source.TryRandomElement(out var newBS))
                        {
                            pawn.story.Childhood = newBS;
                        }
                    }
                    if (pawn.story != null && pawn.story.Adulthood != null && (pawn.story.Adulthood.workDisables & requiredWorkTags) != 0)
                    {
                        var backstoryCategoryFiltersFor = PawnBioAndNameGenerator.GetBackstoryCategoryFiltersFor(pawn, pawn.Faction?.def);
                        var source = DefDatabase<BackstoryDef>.AllDefs.Where((BackstoryDef bs) => bs.shuffleable
                            && bs.slot == BackstorySlot.Adulthood && (bs.workDisables & requiredWorkTags) == 0
                            && backstoryCategoryFiltersFor.Any(x => x.Matches(bs)));
                        if (source.TryRandomElement(out var newBS))
                        {
                            pawn.story.Adulthood = newBS;
                        }
                    }
                    if (pawn.health != null && pawn.health.hediffSet != null)
                    {
                        List<Hediff> hediffsToRemove = new List<Hediff>();
                        foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                        {
                            HediffStage curStage = hediff.CurStage;
                            if (curStage != null && (curStage.disabledWorkTags & requiredWorkTags) != 0)
                            {
                                hediffsToRemove.Add(hediff);
                            }
                        }
                        foreach (var hediff in hediffsToRemove)
                        {
                            pawn.health.RemoveHediff(hediff);
                        }
                    }
                    if (pawn.story.traits != null)
                    {
                        for (int i = pawn.story.traits.allTraits.Count - 1; i >= 0; i--)
                        {
                            if (!pawn.story.traits.allTraits[i].Suppressed)
                            {
                                Trait trait = pawn.story.traits.allTraits[i];
                                if ((trait.def.disabledWorkTags & requiredWorkTags) != 0)
                                {
                                    pawn.story.traits.RemoveTrait(trait);
                                    var newTrait = GenerateTraitFor(pawn, requiredWorkTags);
                                    if (newTrait != null)
                                    {
                                        pawn.story.traits.GainTrait(newTrait);
                                    }
                                }
                            }
                        }
                    }

                    pawn.Notify_DisabledWorkTypesChanged();
                    if (extension.forcedSkills.NullOrEmpty() is false)
                    {
                        foreach (var forcedSkill in extension.forcedSkills)
                        {
                            var skillRecord = __result.skills.GetSkill(forcedSkill.skill);
                            if (skillRecord.TotallyDisabled is false)
                            {
                                skillRecord.Level = forcedSkill.range.RandomInRange;
                            }
                        }
                    }

                    if (extension.skillGains.NullOrEmpty() is false)
                    {
                        foreach (var skillGain in extension.skillGains)
                        {
                            var skillRecord = __result.skills.GetSkill(skillGain.skill);
                            if (skillRecord.TotallyDisabled is false)
                            {
                                var level = Mathf.Clamp(Mathf.RoundToInt(skillRecord.Level + skillGain.range.RandomInRange), 0, 20);
                                skillRecord.Level = level;
                            }
                        }
                    }
                }
            }
        }

        public static Trait GenerateTraitFor(Pawn pawn, WorkTags requiredWorkTags)
        {
            int num = 0;
            while (++num < 500)
            {
                TraitDef newTraitDef = DefDatabase<TraitDef>.AllDefsListForReading.RandomElementByWeight((TraitDef tr) => tr.GetGenderSpecificCommonality(pawn.gender));
                if (pawn.story.traits.HasTrait(newTraitDef) || 
                   (newTraitDef == TraitDefOf.Gay && (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) 
                   || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))) 
                   || (newTraitDef == TraitDefOf.Gay || newTraitDef == TraitDefOf.Bisexual || newTraitDef == TraitDefOf.Asexual))
                {
                    continue;
                }
                if (requiredWorkTags != 0 && (newTraitDef.disabledWorkTags & requiredWorkTags) != 0)
                {
                    continue;
                }
                if (pawn.kindDef.disallowedTraits.NotNullAndContains(newTraitDef) || (pawn.kindDef.disallowedTraitsWithDegree != null 
                    && pawn.kindDef.disallowedTraitsWithDegree.Any((TraitRequirement t) => t.def == newTraitDef && !t.degree.HasValue)) 
                    || (pawn.kindDef.requiredWorkTags != 0 && (newTraitDef.disabledWorkTags & pawn.kindDef.requiredWorkTags) != 0))
                {
                    continue;
                }
                if (pawn.story.traits.allTraits.Any((Trait tr) => newTraitDef.ConflictsWith(tr)) 
                    || (newTraitDef.requiredWorkTypes != null 
                    && pawn.OneOfWorkTypesIsDisabled(newTraitDef.requiredWorkTypes)) 
                    || pawn.WorkTagIsDisabled(newTraitDef.requiredWorkTags) 
                    || (newTraitDef.forcedPassions != null && pawn.workSettings != null 
                    && newTraitDef.forcedPassions.Any((SkillDef p) => p.IsDisabled(pawn.story.DisabledWorkTagsBackstoryTraitsAndGenes, pawn.GetDisabledWorkTypes(permanentOnly: true)))))
                {
                    continue;
                }
                int degree = PawnGenerator.RandomTraitDegree(newTraitDef);
                if ((pawn.story.Childhood == null || !pawn.story.Childhood.DisallowsTrait(newTraitDef, degree)) 
                    && (pawn.story.Adulthood == null || !pawn.story.Adulthood.DisallowsTrait(newTraitDef, degree)))
                {
                    Trait trait = new Trait(newTraitDef, degree);
                    if ((pawn.kindDef.disallowedTraitsWithDegree.NullOrEmpty() || !pawn.kindDef.disallowedTraitsWithDegree.Any((TraitRequirement t) => t.Matches(trait))) && (pawn.mindState == null || pawn.mindState.mentalBreaker == null || !((pawn.mindState.mentalBreaker.BreakThresholdMinor + trait.OffsetOfStat(StatDefOf.MentalBreakThreshold)) * trait.MultiplierOfStat(StatDefOf.MentalBreakThreshold) > 0.5f)))
                    {
                        return trait;
                    }
                }
            }
            return null;
        }
    }
}
