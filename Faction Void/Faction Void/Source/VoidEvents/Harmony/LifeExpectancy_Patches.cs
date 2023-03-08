using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace VoidEvents
{
    [HarmonyPatch(typeof(AgeInjuryUtility), "RandomHediffsToGainOnBirthday", new Type[] { typeof(Pawn), typeof(float) })]
    public static class AgeInjuryUtility_RandomHediffsToGainOnBirthday_Patch
    {
        public static void Prefix(Pawn pawn, float age, out float __state)
        {
            __state = pawn.def.race.lifeExpectancy;
            if (pawn.TryGetChangedLifeExpectancy(out float newLifeExpectancy))
            {
                pawn.def.race.lifeExpectancy = newLifeExpectancy;
            }
        }
        public static void Postfix(Pawn pawn, float age, float __state)
        {
            pawn.def.race.lifeExpectancy = __state;
        }
    }

    [HarmonyPatch(typeof(AgeInjuryUtility), "GenerateRandomOldAgeInjuries")]
    public static class AgeInjuryUtility_GenerateRandomOldAgeInjuries_Patch
    {
        public static void Prefix(Pawn pawn, out float __state)
        {
            __state = pawn.def.race.lifeExpectancy;
            if (pawn.TryGetChangedLifeExpectancy(out float newLifeExpectancy))
            {
                pawn.def.race.lifeExpectancy = newLifeExpectancy;
            }
        }
        public static void Postfix(Pawn pawn, float __state)
        {
            pawn.def.race.lifeExpectancy = __state;
        }
    }

    [HarmonyPatch(typeof(StatPart_Age), "AgeMultiplier")]
    public static class StatPart_Age_AgeMultiplier_Patch
    {
        public static void Prefix(Pawn pawn, out float __state)
        {
            __state = pawn.def.race.lifeExpectancy;
            if (pawn.TryGetChangedLifeExpectancy(out float newLifeExpectancy))
            {
                pawn.def.race.lifeExpectancy = newLifeExpectancy;
            }
        }
        public static void Postfix(Pawn pawn, float __state)
        {
            pawn.def.race.lifeExpectancy = __state;
        }
    }

    [HarmonyPatch(typeof(HediffGiver_RandomAgeCurved), "OnIntervalPassed")]
    public static class HediffGiver_RandomAgeCurved_OnIntervalPassed_Patch
    {
        public static void Prefix(Pawn pawn, out float __state)
        {
            __state = pawn.def.race.lifeExpectancy;
            if (pawn.TryGetChangedLifeExpectancy(out float newLifeExpectancy))
            {
                pawn.def.race.lifeExpectancy = newLifeExpectancy;
            }
        }
        public static void Postfix(Pawn pawn, float __state)
        {
            pawn.def.race.lifeExpectancy = __state;
        }
    }

    [HarmonyPatch(typeof(StatsReportUtility), "DrawStatsReport", new Type[] { typeof(Rect), typeof(Thing) })]
    public static class StatsReportUtility_DrawStatsReport_Patch
    {
        private static void Prefix(Rect rect, Thing thing, out float __state)
        {
            __state = -1;
            if (thing is Pawn pawn && pawn.TryGetChangedLifeExpectancy(out float newLifeExpectancy))
            {
                __state = pawn.def.race.lifeExpectancy;
                pawn.def.race.lifeExpectancy = newLifeExpectancy;
            }
        }

        private static void Postfix(Rect rect, Thing thing, float __state)
        {
            if (__state != -1 && thing is Pawn pawn)
            {
                pawn.def.race.lifeExpectancy = __state;
            }
        }
    }
}