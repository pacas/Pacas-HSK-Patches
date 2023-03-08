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

namespace VoidEvents
{
    class VoidSettings : ModSettings
    {
        public static bool EnableVoidExpansion = true;
        public static bool EnableVoidContact = true;
        public static bool EnableSpawnOfNewVoidBasesNearby = true;
        public static int MaxAmountOfNewVoidBasesNearby = 10;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref EnableVoidExpansion, "EnableVoidExpansion", true);
            Scribe_Values.Look(ref EnableVoidContact, "EnableVoidContact", true);
            Scribe_Values.Look(ref EnableSpawnOfNewVoidBasesNearby, "EnableSpawnOfNewVoidBasesNearby", true);
            Scribe_Values.Look(ref MaxAmountOfNewVoidBasesNearby, "MaxAmountOfNewVoidBasesNearby", 10);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Void.EnableVoidExpansion".Translate(), ref EnableVoidExpansion);
            listingStandard.CheckboxLabeled("Void.EnableSpawnOfNewVoidBasesNearby".Translate(), ref EnableSpawnOfNewVoidBasesNearby);
            listingStandard.SliderLabeled("Void.MaxAmountOfNewVoidBasesNearby".Translate(), ref MaxAmountOfNewVoidBasesNearby,
                MaxAmountOfNewVoidBasesNearby.ToString(), 0, 100);
            listingStandard.CheckboxLabeled("Void.EnableVoidContact".Translate(), ref EnableVoidContact);
            listingStandard.End();
        }
    }
}