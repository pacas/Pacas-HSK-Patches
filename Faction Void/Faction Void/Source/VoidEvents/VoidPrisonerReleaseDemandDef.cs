using EncounterFramework;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace VoidEvents
{

    public class VoidPrisonerReleaseDemandDef : Def
    {
        public string voidStartingTitle;
        public string voidStartingText;
        public string thankYouMessageTitle;
        public string thankYouMessageText;
        public int raidsAfterTicks;
        public List<ThreatOption> raids;
        public int raidIntervalTicks;
        public int prisonerReleaseDemandCooldownTicks;
    }
}