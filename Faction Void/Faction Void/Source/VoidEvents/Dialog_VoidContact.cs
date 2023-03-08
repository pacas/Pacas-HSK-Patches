using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace VoidEvents
{
    public class Dialog_VoidContact : Dialog_NodeTree
    {
        private Pawn contacter;
        private DiaOption begOption;
        private DiaOption respectfulDeclineOption;
        private DiaOption disrespectOption;
        public Dialog_VoidContact(Pawn contacter, DiaNode nodeRoot, bool delayInteractivity = false, bool radioMode = false, string title = null) : base(nodeRoot, delayInteractivity, radioMode, title)
        {
            this.contacter = contacter;
            begOption = new DiaOption(VoidDefOf.VoidContact.begOptionText)
            {
                dialog = this,
                action = delegate ()
                {
                    GiveGifts();
                    var comp = Current.Game.GetComponent<VoidGameComp>();
                    if (Rand.Chance(VoidDefOf.VoidContact.begSuccessChance))
                    {
                        contacter.GetLord().Map.lordManager.RemoveLord(contacter.GetLord());
                        Current.Game.GetComponent<VoidGameComp>().neutralUntilTicks = Find.TickManager.TicksGame
                            + (VoidDefOf.VoidContact.netralPeriodAfterBeg * GenDate.TicksPerDay);
                    }
                    else
                    {
                        DeclineAction();
                    }
                },
                link = AddResponse(VoidDefOf.VoidContact.begOptionResponses.RandomElement())
            };
            respectfulDeclineOption = new DiaOption(VoidDefOf.VoidContact.respectfulDeclineOptionText)
            {
                dialog = this,
                action = delegate ()
                {
                    GiveGifts();
                    DeclineAction();
                },
                link = AddResponse(VoidDefOf.VoidContact.respectfulDeclineOptionResponses.RandomElement())
            };
            disrespectOption = new DiaOption(VoidDefOf.VoidContact.disrespectOptionText)
            {
                dialog = this,
                action = delegate ()
                {
                    var comp = Current.Game.GetComponent<VoidGameComp>();
                    comp.SetVoidAsPermanentEnemy();
                    var lord = contacter.GetLord();
                    var pawns = lord.ownedPawns;
                    lord.Map.lordManager.RemoveLord(lord);
                    foreach (var pawn in pawns)
                    {
                        pawn.jobs.StopAll();
                    }
                    LordMaker.MakeNewLord(contacter.Faction, new LordJob_AssaultColony(contacter.Faction), contacter.Map, pawns);
                },
                link = AddResponse(VoidDefOf.VoidContact.disrespectOptionResponses.RandomElement())
            };
            nodeRoot.options.Add(begOption);
            nodeRoot.options.Add(respectfulDeclineOption);
            nodeRoot.options.Add(disrespectOption);
        }

        private static DiaNode AddResponse(string response)
        {
            return new DiaNode(response)
            {
                options = new List<DiaOption>
                {
                    new DiaOption("Close".Translate())
                    {
                        resolveTree = true,
                    }
                }
            };
        }

        private void DeclineAction()
        {
            contacter.GetLord().Map.lordManager.RemoveLord(contacter.GetLord());
            Current.Game.GetComponent<VoidGameComp>().neutralUntilTicks = Find.TickManager.TicksGame
                + (VoidDefOf.VoidContact.netralPeriodAfterRespectfulDecline * GenDate.TicksPerDay);
            foreach (var incident in VoidDefOf.VoidContact.incidentsAfterDeclining)
            {
                var parms = StorytellerUtility.DefaultParmsNow(incident.category, contacter.Map);
                parms.faction = contacter.Faction;
                Find.Storyteller.incidentQueue.Add(incident, Find.TickManager.TicksGame + VoidDefOf.VoidContact.incidentsAfterDecliningInTicks, parms);
            }
        }

        private void GiveGifts()
        {
            var gifts = new List<Thing>();
            foreach (var option in VoidDefOf.VoidContact.giftOptions)
            {
                int toMake = option.count;
                while (toMake > 0)
                {
                    int countToMake = Mathf.Min(toMake, option.gift.stackLimit);
                    toMake -= countToMake;
                    var gift = ThingMaker.MakeThing(option.gift);
                    gift.stackCount = countToMake;
                    gifts.Add(gift);
                }
            }
            VisitorGiftForPlayerUtility.GiveGift(new List<Pawn> { contacter }, contacter.Faction, gifts);
        }
    }
}