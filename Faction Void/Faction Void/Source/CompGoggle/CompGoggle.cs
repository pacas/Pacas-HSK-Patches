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
using Verse.Sound;
using Verse.AI;

namespace NightVisionGoggle
{
	public class CompProperties_Goggle : CompProperties
    {
        public string goggleOnGroundTexPath;
        public string wornGoggleTexPath;
        public SoundDef nightVisionEnabledSound;
        public bool autoToggle;
        public CompProperties_Goggle()
        {
            this.compClass = typeof(CompGoggle);
        }
    }
	public class CompGoggle : ThingComp
	{
        public static HashSet<Thing> goggleThings = new HashSet<Thing>
        {

        };
        private string apparelOnGroundTexPath;
        private string wornApparelTexPath;
        public bool goggleIsOn;

        public void SetState(bool state)
        {
            Log.Message(Wearer + " setting state: " + state + " - " + Wearer.needs.mood.recentMemory.TicksSinceLastLight);
            if (state)
            {
                apparelOnGroundTexPath = this.Props.goggleOnGroundTexPath;
                wornApparelTexPath = this.Props.wornGoggleTexPath;
                Props.nightVisionEnabledSound?.PlayOneShotOnCamera();
            }
            else
            {
                apparelOnGroundTexPath = this.parent.def.graphicData.texPath;
                wornApparelTexPath = this.parent.def.apparel.wornGraphicPath;
            }
            ChangeGraphic(apparelOnGroundTexPath);
            ChangeWornGraphic();
            goggleIsOn = state;
        }
        public CompProperties_Goggle Props => this.props as CompProperties_Goggle;

        public Apparel Apparel => this.parent as Apparel;
        public Pawn Wearer => Apparel.Wearer;
        public bool NightVisionWorks()
        {
            var pawn = Wearer;
            if (pawn != null)
            {
                return pawn.needs.mood.recentMemory.TicksSinceLastLight > 240;
            }
            return false;
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad && !Props.goggleOnGroundTexPath.NullOrEmpty())
            {
                apparelOnGroundTexPath = this.parent.def.graphicData.texPath;
                wornApparelTexPath = this.parent.def.apparel.wornGraphicPath;
            }
            if (!Props.goggleOnGroundTexPath.NullOrEmpty())
            {
                ChangeGraphic(apparelOnGroundTexPath);
                ChangeWornGraphic();
            }
            goggleThings.Add(this.parent);
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            goggleThings.Add(this.parent);
        }

        public void ChangeGraphic(string texPath)
        {
            if (!texPath.NullOrEmpty())
            {
                var graphicData = new GraphicData();
                graphicData.graphicClass = this.parent.def.graphicData.graphicClass;
                graphicData.texPath = texPath;
                graphicData.shaderType = this.parent.def.graphicData.shaderType;
                graphicData.drawSize = this.parent.def.graphicData.drawSize;
                graphicData.color = this.parent.def.graphicData.color;
                graphicData.colorTwo = this.parent.def.graphicData.colorTwo;
                var newGraphic = graphicData.GraphicColoredFor(this.parent);
                Traverse.Create(this.parent).Field("graphicInt").SetValue(newGraphic);
                base.parent.Map?.mapDrawer?.MapMeshDirty(this.parent.Position, MapMeshFlag.Things);
            }
        }

        public void ChangeWornGraphic()
        {
            if (this.parent is Apparel apparel)
            {
                apparel.Wearer?.Drawer.renderer.graphics.ResolveApparelGraphics();
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (Props.autoToggle)
            {
                var pawn = Wearer;
                if (pawn != null)
                {
                    if (goggleIsOn && pawn.needs.mood.recentMemory.TicksSinceLastLight < 240)
                    {
                        SetState(false);
                    }
                    else if (!goggleIsOn && pawn.needs.mood.recentMemory.TicksSinceLastLight > 240)
                    {
                        SetState(true);
                    }
                }
            }
        }
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (var g in base.CompGetWornGizmosExtra())
            {
                yield return g;
            }
            if (!Props.autoToggle && !Props.goggleOnGroundTexPath.NullOrEmpty())
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = this.goggleIsOn ? "Void_DisableNightVision".Translate() : "Void_EnableNightVision".Translate();
                command_Action.icon = ContentFinder<Texture2D>.Get("Things/UI/Goggle");
                command_Action.action = delegate
                {
                    SetState(!goggleIsOn);
                };
                yield return command_Action;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref apparelOnGroundTexPath, "apparelOnGroundTexPath");
            Scribe_Values.Look(ref wornApparelTexPath, "wornApparelTexPath");
            Scribe_Values.Look(ref goggleIsOn, "goggleIsOn");
            goggleThings.Add(this.parent);
        }
    }
}
