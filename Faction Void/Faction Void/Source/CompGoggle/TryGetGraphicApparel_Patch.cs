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

namespace NightVisionGoggle
{
	[StaticConstructorOnStartup]
	static class HarmonyContainer
	{
		static HarmonyContainer()
		{
			Harmony harmony = new Harmony("NightVisionGoggle.CompGoggle");
			harmony.PatchAll();
		}
	}

	[HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
    class TryGetGraphicApparel_Patch
    {
        public static bool Prefix(ref bool __result, Apparel apparel, BodyTypeDef bodyType, ref ApparelGraphicRecord rec)
        {
            var comp = apparel.TryGetComp<CompGoggle>();
            if (comp?.goggleIsOn ?? false)
            {
				__result = TryGetGraphicApparel(apparel, bodyType, comp, out rec);
				return false;
			}
			return true;
        }
		public static bool TryGetGraphicApparel(Apparel apparel, BodyTypeDef bodyType, CompGoggle comp, out ApparelGraphicRecord rec)
		{
			if (bodyType == null)
			{
				Log.Error("Getting apparel graphic with undefined body type.");
				bodyType = BodyTypeDefOf.Male;
			}
			if (apparel.def.apparel.wornGraphicPath.NullOrEmpty())
			{
				rec = new ApparelGraphicRecord(null, null);
				return false;
			}
			var wornPath = comp.Props.wornGoggleTexPath;
			string path = (apparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead && !PawnRenderer.RenderAsPack(apparel) 
				&& !(wornPath == BaseContent.PlaceholderImagePath)) ? (wornPath + "_" + bodyType.defName) : wornPath;
			Shader shader = ShaderDatabase.Cutout;
			if (apparel.def.apparel.useWornGraphicMask)
			{
				shader = ShaderDatabase.CutoutComplex;
			}
			Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
			rec = new ApparelGraphicRecord(graphic, apparel);
			return true;
		}
	}
}