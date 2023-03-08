using Enlist;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VoidEvents
{
    public class VoidGameComp : GameComponent
    {
        public bool voidVisited;
        public Map voidPrisonerReleaseMapRaid;
        public int voidPrisonerReleaseDeadlineTicks = -1;
        public int lastVoidPrisonerReleaseDemandsTicks = -1;
        public int voidPrisonerReleaseRaidIndex = -1;
        public int tickToNextBaseConversion = 0;
        public Dictionary<Map, int> voidReinforcementsToSend;
        public List<PawnToResurrect> pawnsToResurrect;
        public List<Hediff_Regen> hediff_Regenerations;
        public List<Settlement> newVoidBases;
        private int prevDay;
        public bool isPermanentEnemy;
        public int neutralUntilTicks = -1;
        public bool enlistedAlready;
        public int argentProtocolTicks = -1;
        public VoidGameComp()
        {
            PreInit();
        }
        public VoidGameComp(Game game)
        {
            PreInit();
        }

        public void PreInit()
        {
            if (voidReinforcementsToSend == null)
            {
                voidReinforcementsToSend = new Dictionary<Map, int>();
            }
            if (newVoidBases is null)
            {
                newVoidBases = new List<Settlement>();
            }

            if (pawnsToResurrect == null)
            {
                pawnsToResurrect = new List<PawnToResurrect>();
            }

            if (hediff_Regenerations == null)
            {
                hediff_Regenerations = new List<Hediff_Regen>();
            }
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            PreInit();
            CheckPermanentEnemyStatus();
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            PreInit();
            CheckPermanentEnemyStatus();
            tickToNextBaseConversion = Find.TickManager.TicksAbs + new IntRange(7 * GenDate.TicksPerDay, 30 * GenDate.TicksPerDay).RandomInRange;
        }

        private void CheckPermanentEnemyStatus()
        {
            VoidDefOf.RH_VOID.permanentEnemy = !IsEnlistedToVoid() && isPermanentEnemy;
        }

        public static bool IsEnlistedToVoid()
        {
            Faction voidFaction = Find.FactionManager.FirstFactionOfDef(VoidDefOf.RH_VOID);
            foreach (var enlistOption in voidFaction.GetEnlistOptions())
            {
                if (WorldEnlistTracker.Instance.EnlistedTo(voidFaction, enlistOption))
                {
                    return true;
                }
            }
            return false;
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            Faction voidFaction = Find.FactionManager.FirstFactionOfDef(VoidDefOf.RH_VOID);
            if (voidFaction is null)
            {
                voidFaction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(VoidDefOf.RH_VOID));
                Find.FactionManager.Add(voidFaction);
            }

            if (VoidSettings.EnableVoidExpansion)
            {
                if (Find.TickManager.TicksAbs > tickToNextBaseConversion)
                {
                    tickToNextBaseConversion = Find.TickManager.TicksAbs + new IntRange(7 * GenDate.TicksPerDay, 30 * GenDate.TicksPerDay).RandomInRange;
                    Settlement randomVoidBase = Find.WorldObjects.SettlementBases.Where(x => x.Faction == voidFaction).FirstOrDefault();
                    if (randomVoidBase == null)
                    {
                        Settlement settlementToConvert = Find.WorldObjects.SettlementBases.Where(x => x.Faction.def != Faction.OfPlayer.def
                            && x.Faction != voidFaction).InRandomOrder().FirstOrDefault();
                        if (settlementToConvert != null)
                        {
                            ConvertSettlement(settlementToConvert, voidFaction);
                        }
                    }
                    else
                    {
                        Settlement settlementToConvert = Find.WorldObjects.SettlementBases.Where(x => x.Faction.def != Faction.OfPlayer.def
                            && x.Faction != voidFaction).OrderBy(x => Find.WorldGrid.ApproxDistanceInTiles(randomVoidBase.Tile, x.Tile)).FirstOrDefault();
                        if (settlementToConvert != null)
                        {
                            ConvertSettlement(settlementToConvert, voidFaction);
                        }
                    }

                }
                if (voidReinforcementsToSend.Count > 0)
                {

                    foreach (KeyValuePair<Map, int> data in voidReinforcementsToSend)
                    {
                        if (Find.TickManager.TicksAbs >= data.Value)
                        {
                            if (data.Key.ParentFaction != Faction.OfPlayer)
                            {
                                IncidentParms parms = new IncidentParms
                                {
                                    target = data.Key,
                                    points = StorytellerUtility.DefaultThreatPointsNow(Find.World),
                                    faction = voidFaction
                                };
                                IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
                                voidReinforcementsToSend[data.Key] = Find.TickManager.TicksAbs + new IntRange(GenDate.TicksPerHour * 2, GenDate.TicksPerHour * 3).RandomInRange;
                            }
                        }
                    }
                }
            }

            if (voidPrisonerReleaseDeadlineTicks > 0)
            {
                if (Find.TickManager.TicksGame >= voidPrisonerReleaseDeadlineTicks)
                {
                    if (Find.Maps.Where(x => x.IsPlayerHome).Any(y => y.mapPawns.PrisonersOfColony.Any(p => p.Faction == voidFaction)))
                    {
                        if (voidPrisonerReleaseRaidIndex == VoidDefOf.VoidPrisonerReleaseDemand.raids.Count())
                        {
                            voidPrisonerReleaseDeadlineTicks = -1;
                            voidPrisonerReleaseRaidIndex = -1;
                        }
                        else
                        {
                            VoidDefOf.VoidPrisonerReleaseDemand.raids[voidPrisonerReleaseRaidIndex].GenerateThreat(voidPrisonerReleaseMapRaid);
                            voidPrisonerReleaseRaidIndex++;
                            voidPrisonerReleaseDeadlineTicks += VoidDefOf.VoidPrisonerReleaseDemand.raidIntervalTicks;
                        }
                    }
                    else
                    {
                        Find.LetterStack.ReceiveLetter(VoidDefOf.VoidPrisonerReleaseDemand.thankYouMessageTitle,
                            VoidDefOf.VoidPrisonerReleaseDemand.thankYouMessageText, LetterDefOf.PositiveEvent);
                        voidPrisonerReleaseDeadlineTicks = -1;
                        voidPrisonerReleaseRaidIndex = -1;
                    }
                }
            }
            else if ((lastVoidPrisonerReleaseDemandsTicks <= 0 || Find.TickManager.TicksGame - lastVoidPrisonerReleaseDemandsTicks
                >= VoidDefOf.VoidPrisonerReleaseDemand.prisonerReleaseDemandCooldownTicks) && Find.TickManager.TicksGame % 2500 == 0)
            {
                foreach (var playerMap in Find.Maps.Where(x => x.IsPlayerHome))
                {
                    var prisoners = playerMap.mapPawns.PrisonersOfColony.Where(x => x.Faction == voidFaction).ToList();
                    if (prisoners.Any() && playerMap.mapPawns.SpawnedPawnsInFaction(voidFaction)
                        .Any(x => x.HostileTo(Faction.OfPlayer)) is false)
                    {
                        Find.LetterStack.ReceiveLetter(VoidDefOf.VoidPrisonerReleaseDemand.voidStartingTitle,
                            VoidDefOf.VoidPrisonerReleaseDemand.voidStartingText, LetterDefOf.NegativeEvent, prisoners);
                        voidPrisonerReleaseMapRaid = playerMap;
                        voidPrisonerReleaseDeadlineTicks = Find.TickManager.TicksGame + VoidDefOf.VoidPrisonerReleaseDemand.raidsAfterTicks;
                        voidPrisonerReleaseRaidIndex = 0;
                        lastVoidPrisonerReleaseDemandsTicks = Find.TickManager.TicksGame;
                    }
                }
            }

            Map localMap = Find.AnyPlayerHomeMap;
            if (localMap != null && VoidSettings.EnableSpawnOfNewVoidBasesNearby && VoidSettings.EnableVoidExpansion)
            {
                int day = GenDate.DayOfYear(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(localMap.Tile).x);
                if (prevDay != day && day == 59)
                {
                    IEnumerable<Settlement> voidSettlements = Find.WorldObjects.SettlementBases.Where(x => x.Faction == voidFaction);
                    newVoidBases.RemoveAll(x => x is null || x.Destroyed);
                    if (voidSettlements.Any() && newVoidBases.Count < VoidSettings.MaxAmountOfNewVoidBasesNearby && TryFindNewSettlementTile(out int tile, 5, 7))
                    {
                        Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                        settlement.SetFaction(voidFaction);
                        settlement.Tile = tile;
                        settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement);
                        Find.WorldObjects.Add(settlement);
                        newVoidBases.Add(settlement);
                    }
                }
                prevDay = day;
            }

            if (VoidSettings.EnableVoidContact && Find.TickManager.TicksGame % GenDate.TicksPerHour == 0)
            {
                var parms = StorytellerUtility.DefaultParmsNow(VoidDefOf2.Void_Contact.category, localMap);
                parms.faction = voidFaction;
                parms.target = localMap;
                if (IncidentWorker_VoidContact.ShouldExecute(parms))
                {
                    VoidDefOf2.Void_Contact.Worker.TryExecute(parms);
                }
            }

            if (neutralUntilTicks != -1 && Find.TickManager.TicksGame >= neutralUntilTicks)
            {
                if (IsEnlistedToVoid() is false)
                {
                    SetVoidAsPermanentEnemy();
                    Find.LetterStack.ReceiveLetter("Void.Hostile".Translate(), "Void.HostileDesc".Translate(), LetterDefOf.ThreatBig);
                }
                neutralUntilTicks = -1;
            }

            if (argentProtocolTicks != -1 && Find.TickManager.TicksGame >= argentProtocolTicks)
            {
                foreach (var faction in Find.FactionManager.AllFactions)
                {
                    if (faction.def.naturalEnemy || faction.def.permanentEnemy)
                    {
                        continue;
                    }
                    if (voidFaction.GoodwillWith(faction) < 0)
                    {
                        voidFaction.ChangeRelation(0, faction);
                    }
                }
                Find.LetterStack.ReceiveLetter("Void.ProjectArgentInitiatedTitle".Translate(), "Void.ProjectArgentInitiatedText".Translate(), LetterDefOf.PositiveEvent);
                argentProtocolTicks = -1;
            }

            for (int num = hediff_Regenerations.Count - 1; num >= 0; num--)
            {
                var hediff = hediff_Regenerations[num];
                if (hediff != null)
                {
                    hediff.TryHealRandomPart();
                }
                hediff_Regenerations.RemoveAt(num);
            }

            if (pawnsToResurrect.Count > 0)
            {
                List<PawnToResurrect> pawnKeys = new List<PawnToResurrect>();
                foreach (PawnToResurrect data in pawnsToResurrect)
                {
                    if (Find.TickManager.TicksAbs >= data.tickToResurrect)
                    {
                        if (data.pawnToResurrect?.Corpse != null)
                        {
                            if (data.pawnToResurrect.Corpse.ParentHolder is Building_Grave grave)
                            {
                                grave.Open();
                            }
                            else if (data.pawnToResurrect.Corpse.holdingOwner != null && data.pawnToResurrect.Corpse.Spawned is false)
                            {
                                var map = data.pawnToResurrect.Corpse.MapHeld;
                                if (map != null)
                                {
                                    var pos = data.pawnToResurrect.Corpse.PositionHeld;
                                    data.pawnToResurrect.Corpse.holdingOwner.Remove(data.pawnToResurrect.Corpse);
                                    GenSpawn.Spawn(data.pawnToResurrect.Corpse, pos, map);
                                }
                            }

                            ResurrectionUtility.Resurrect(data.pawnToResurrect);
                            List<Hediff> tmpHediffsToTend = new List<Hediff>();
                            List<Hediff> hediffs = data.pawnToResurrect.health.hediffSet.hediffs;
                            for (int i = 0; i < hediffs.Count; i++)
                            {
                                if (hediffs[i].TendableNow() || hediffs[i] is Hediff_Addiction || hediffs[i] is Hediff_Injury
                                    || (hediffs[i] is HediffWithComps withComps && withComps.TryGetComp<HediffComp_Immunizable>() != null)
                                    || hediffs[i].def.makesSickThought 
                                    || hediffs[i] is not Hediff_Implant && hediffs[i].CapMods.Any(x => x.capacity == PawnCapacityDefOf.Consciousness && (x.offset < 0 || x.setMax < 1)))
                                {
                                    tmpHediffsToTend.Add(hediffs[i]);
                                }
                            }
                            foreach (Hediff hediff in tmpHediffsToTend)
                            {
                                for (int i = 0; i < tmpHediffsToTend.Count; i++)
                                {
                                    data.pawnToResurrect.health.RemoveHediff(tmpHediffsToTend[i]);
                                }
                            }

                            if (data.pawnToResurrect.Spawned && data.pawnToResurrect.equipment.Primary is null 
                                && data.pawnToResurrect.kindDef == VoidDefOf.RH_VOID_Undying)
                            {
                                var firstWeapon = data.pawnToResurrect.inventory.GetDirectlyHeldThings()
                                    .OfType<ThingWithComps>().FirstOrDefault(x => x.def.IsWeapon);
                                if (firstWeapon != null)
                                {
                                    if (firstWeapon.holdingOwner != null)
                                    {
                                        firstWeapon.holdingOwner.Remove(firstWeapon);
                                    }
                                    data.pawnToResurrect.equipment.MakeRoomFor(firstWeapon);
                                    data.pawnToResurrect.equipment.AddEquipment(firstWeapon);
                                }
                                else
                                {
                                    var jbg = new JobGiver_PickUpOpportunisticWeapon();
                                    var job = jbg.TryGiveJob(data.pawnToResurrect);
                                    if (job != null)
                                    {
                                        data.pawnToResurrect.jobs.TryTakeOrderedJob(job);
                                    }
                                }
                            }

                            PortraitsCache.SetDirty(data.pawnToResurrect);
                            if (data.pawnToResurrect.IsWorldPawn())
                            {
                                List<int> neigbors = new List<int>();
                                Find.WorldGrid.GetTileNeighbors(data.tile, neigbors);
                                int dest = neigbors.RandomElement();
                                CaravanFormingUtility.FormAndCreateCaravan(new List<Pawn> { data.pawnToResurrect }, Faction.OfPlayer, data.tile, dest, dest);
                                Find.LetterStack.ReceiveLetter("Void_ResurrectionLabel".Translate(data.pawnToResurrect.Named("PAWN")),
                                    "Void_ResurrectionDesc".Translate(data.pawnToResurrect.Named("PAWN")), LetterDefOf.PositiveEvent, data.pawnToResurrect);
                            }
                            pawnKeys.Add(data);
                        }
                    }
                }
                foreach (PawnToResurrect key in pawnKeys)
                {
                    pawnsToResurrect.Remove(key);
                }
            }
        }

        public void SetVoidAsPermanentEnemy()
        {
            Faction voidFaction = Find.FactionManager.FirstFactionOfDef(VoidDefOf.RH_VOID);
            voidFaction.ChangeRelation(-80);
            isPermanentEnemy = VoidDefOf.RH_VOID.permanentEnemy = true;
        }

        public static bool TryFindNewSettlementTile(out int tile, int minDist = 7, int maxDist = 27, bool allowCaravans = false, TileFinderMode tileFinderMode = TileFinderMode.Near, int nearThisTile = -1, bool exitOnFirstTileFound = false)
        {
            int findTile(int root)
            {
                return TileFinder.TryFindPassableTileWithTraversalDistance(root, minDist, maxDist, out int result, (int x) =>
                    !Find.WorldObjects.AnyWorldObjectAt(x) && TileFinder.IsValidTileForNewSettlement(x)
                    && VoidSettlementNearby(Find.AnyPlayerHomeMap.Tile), ignoreFirstTilePassability: false, tileFinderMode, canTraverseImpassable: false, exitOnFirstTileFound)
                    ? result
                    : TileFinder.TryFindPassableTileWithTraversalDistance(root, minDist, maxDist, out result, (int x) =>
                !Find.WorldObjects.AnyWorldObjectAt(x) && TileFinder.IsValidTileForNewSettlement(x), ignoreFirstTilePassability: false, tileFinderMode, canTraverseImpassable: true, exitOnFirstTileFound) ? result : (-1);
            }
            int tile2;
            if (nearThisTile != -1)
            {
                tile2 = nearThisTile;
            }
            else if (!TileFinder.TryFindRandomPlayerTile(out tile2, allowCaravans, (int x) => findTile(x) != -1))
            {
                tile = -1;
                return false;
            }
            tile = findTile(tile2);
            return tile != -1;
        }

        public static bool VoidSettlementNearby(int tile)
        {
            List<Settlement> voidSettlements = Find.WorldObjects.Settlements
                .Where(x => x.Faction == Find.FactionManager.FirstFactionOfDef(VoidDefOf.RH_VOID)).ToList();
            return voidSettlements.Any(x => Find.WorldGrid.ApproxDistanceInTiles(x.Tile, tile) <= 5f);
        }
        public void ConvertSettlement(Settlement settlementToConvert, Faction voidFaction)
        {
            Faction defeatedFaction = settlementToConvert.Faction;
            Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
            settlement.SetFaction(voidFaction);
            settlement.Tile = settlementToConvert.Tile;
            settlement.Name = settlementToConvert.Name;
            Find.WorldObjects.Add(settlement);
            Find.LetterStack.ReceiveLetter("Void.SettlementIsDefeated".Translate(), "Void.SettlementIsDefeatedDesc".Translate(settlementToConvert.Name, settlementToConvert.Faction.Named("FACTION"))
                , VoidDefOf.Void_ThreatBig, settlement);
            settlementToConvert.Destroy();
            if (!Find.WorldObjects.SettlementBases.Where(x => x.Faction == defeatedFaction).Any())
            {
                defeatedFaction.defeated = true;
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref tickToNextBaseConversion, "tickToNextBaseConversion");
            Scribe_Collections.Look<Map, int>(ref voidReinforcementsToSend, "voidReinforcementsToSend", LookMode.Reference, LookMode.Value, ref workMaps, ref workInts);
            Scribe_Collections.Look<PawnToResurrect>(ref pawnsToResurrect, "pawnsToBeResurrected", LookMode.Deep);
            Scribe_Collections.Look(ref hediff_Regenerations, "hediff_Regenerations", LookMode.Reference);
            Scribe_Collections.Look(ref newVoidBases, "newVoidBases", LookMode.Reference);
            Scribe_Values.Look(ref prevDay, "prevDay");
            Scribe_Values.Look(ref isPermanentEnemy, "isPermanentEnemy");
            Scribe_Values.Look(ref voidVisited, "voidVisited");
            Scribe_Values.Look(ref neutralUntilTicks, "neutralUntilTicks", -1);
            Scribe_Values.Look(ref enlistedAlready, "enlistedAlready");
            Scribe_Values.Look(ref voidPrisonerReleaseDeadlineTicks, "voidPrisonerReleaseDeadlineTicks", -1);
            Scribe_Values.Look(ref voidPrisonerReleaseRaidIndex, "voidPrisonerReleaseRaidIndex", -1);
            Scribe_Values.Look(ref lastVoidPrisonerReleaseDemandsTicks, "lastVoidPrisonerReleaseDemandsTicks", -1);
            Scribe_References.Look(ref voidPrisonerReleaseMapRaid, "voidPrisonerReleaseMapRaid");
            Scribe_Values.Look(ref argentProtocolTicks, "argentProtocolTicks", -1);
            PreInit();
        }

        private List<Map> workMaps;
        private List<int> workInts;
    }
}