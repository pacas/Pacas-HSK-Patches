using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI.Group;

namespace VoidEvents
{
    [HarmonyPatch(typeof(Pawn), "SpawnSetup")]
    class SpawnSetup_Patch
    {
        public static void Postfix(Pawn __instance)
        {
            if (__instance.kindDef.HasModExtension<SpaceProtection>() && __instance.health.hediffSet.GetFirstHediffOfDef(VoidDefOf.Void_SpaceProtection) == null)
            {
                var hediff = HediffMaker.MakeHediff(VoidDefOf.Void_SpaceProtection, __instance);
                __instance.health.AddHediff(hediff);
            }
        }
    }
}