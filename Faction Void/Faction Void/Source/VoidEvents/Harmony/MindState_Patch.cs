using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI.Group;
using Verse.AI;

namespace VoidEvents
{
    [HarmonyPatch(typeof(Pawn_MindState))]
    [HarmonyPatch("CanStartFleeingBecauseOfPawnAction")]
    internal static class DisableMutantFlee
    {
        private static bool Prefix(ref bool __result, Pawn p)
        {
            if (p is Mutant)
            {
                __result = false;
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}