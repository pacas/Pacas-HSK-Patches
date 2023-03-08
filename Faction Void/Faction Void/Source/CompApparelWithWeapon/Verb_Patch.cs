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
using Verse.Sound;
using Verse.AI;

namespace VoidEvents
{
    [StaticConstructorOnStartup]
    static class HarmonyContainer
    {
        static HarmonyContainer()
        {
            Harmony harmony = new Harmony("com.dninemfive.chickenVOID");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
        class TryCastNextBurstShot_Patch
        {
            public static bool Prefix(Verb __instance)
            {
                if (__instance is Verb_LaunchProjectileApparelWeapon verb)
                {
                    verb.TryCastNextBurstShotCustom();
                    return false;
                }
                return true;
            }
        }
    }
}