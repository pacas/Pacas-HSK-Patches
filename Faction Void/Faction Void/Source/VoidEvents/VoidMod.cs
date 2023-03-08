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
    class VoidMod : Mod
    {
        public const int VoidEnlistPassword = -82541464;

        public static VoidSettings settings;
        public VoidMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<VoidSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "[RH] Faction: VOID";
        }
    }
}