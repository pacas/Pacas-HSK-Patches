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

namespace VoidEvents
{
    class DefoliatorShipPart : Building
    {
        public int tickToSpawnHostiles = 0;
        public bool spawnOnce = false;
        public ShipPawnTypes options;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            options = this.def.GetModExtension<ShipPawnTypes>();
            if (!respawningAfterLoad)
            {
                tickToSpawnHostiles = Find.TickManager.TicksAbs + new IntRange(4 * GenDate.TicksPerDay, 7 * GenDate.TicksPerDay).RandomInRange;
            }
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (this.Map != null)
            {
                SpawnThreats();
            }
            base.PostApplyDamage(dinfo, totalDamageDealt);
        }

        public void SpawnThreats()
        {
            if (!spawnOnce)
            {
                var chosenPawnType = options.pawnSpawnOptions.RandomElement();
                var pawns = new List<Pawn>();
                var voidFaction = Find.FactionManager.FirstFactionOfDef(VoidDefOf.RH_VOID);
                var faction = chosenPawnType.factionDef != null ? Find.FactionManager.FirstFactionOfDef(chosenPawnType.factionDef)
                                        ?? voidFaction : voidFaction;
                for (int i = 0; i < chosenPawnType.amount; i++)
                {
                    var pawn = PawnGenerator.GeneratePawn(chosenPawnType.pawnKindDef, faction);
                    pawns.Add(pawn);
                    GenSpawn.Spawn(pawn, this.Position, this.Map);
                    LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(faction), this.Map, pawns);
                }
                spawnOnce = true;
            }


        }
        public override void Tick()
        {
            base.Tick();
            if (tickToSpawnHostiles == Find.TickManager.TicksAbs)
            {
                SpawnThreats();
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString() + "\n");
            Vector2 vector = Find.WorldGrid.LongLatOf(this.Map.Tile);
            stringBuilder.Append("Void.tickToSpawnHostiles".Translate() + GenDate.DateReadoutStringAt((long)this.tickToSpawnHostiles, vector));
            return stringBuilder.ToString().TrimEndNewlines();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref tickToSpawnHostiles, "tickToSpawnHostiles");
            Scribe_Values.Look(ref spawnOnce, "spawnOnce");
        }
    }
}