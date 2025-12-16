using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using UnityEngine.UI;
using Verse;

namespace SimpleMercenaries.Core
{
    public class Company : ICommunicable, ITrader, IThingHolder, IExposable
    {
        private IncidentParms_MercenariesHired incidentParmsMercenariesHired = null;

        private Faction faction = null;

        private int lastMercGenTime = 0;

        private ThingOwner things;

        private int randomPriceFactorSeed = -1;

        private Map map = null;

        public CompanyDef def = null;

        public IncidentParms_MercenariesHired IncidentParmsMercenariesHired
        {
            get
            {
                return incidentParmsMercenariesHired;
            }
        }


        public TraderKindDef TraderKind
        {
            get
            {
                return def.traderKindDef;
            }
        }

        public IEnumerable<Thing> Goods
        {
            get
            {
                for (int i = 0; i < things.Count; i++)
                {
                    yield return things[i];
                }
            }
        }

        public int RandomPriceFactorSeed
        {
            get
            {
                return randomPriceFactorSeed;
            }
        }

        public string TraderName
        {
            get
            {
                return GetCallLabel();
            }
        }

        public bool CanTradeNow
        {
            get
            {
                return true;
            }
        }

        public float TradePriceImprovementOffsetForPlayer
        {
            get
            {
                return 0f;
            }
        }

        public Faction Faction
        {
            get
            {
                return faction;
            }
        }

        public TradeCurrency TradeCurrency
        {
            get
            {
                return def.traderKindDef.tradeCurrency;
            }
        }

        public IThingHolder ParentHolder
        {
            get
            {
                return map;
            }
        }

        public Company() {}

        public Company(CompanyDef def)
        {
            this.def = def;
            this.faction = Find.FactionManager.AllFactions.Single(f => f.def.defName == this.def.factionDef.defName);
        }

        public string GetCallLabel()
        {
            return Faction.Name;
        }

        public string GetInfoText()
        {
            return def.label;
        }

        public void TryOpenComms(Pawn negotiator)
        {
            InitCompany(negotiator);

            Dialog_Negotiation dialog_Negotiation = new Dialog_Negotiation(negotiator, this, CompanyDialogMaker.DialogFor(negotiator, GetFaction()), true);
            dialog_Negotiation.soundAmbient = SoundDefOf.RadioComms_Ambience;
            Find.WindowStack.Add(dialog_Negotiation);
        }

        public Faction GetFaction()
        {
            return Faction;
        }

        public FloatMenuOption CommFloatMenuOption(Building_CommsConsole console, Pawn negotiator)
        {
            string text = "CallOnRadio".Translate(GetCallLabel());

            return FloatMenuUtility.DecoratePrioritizedTask(
                new FloatMenuOption(
                    text,
                    delegate{console.GiveUseCommsJob(negotiator, this);},
                    def.factionDef.FactionIcon,
                    Faction.Color,
                    MenuOptionPriority.InitiateSocial
                ),
                negotiator,
                console
            );
        }

        public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
        {
            if(def.orbital)
            {
                return ColonyThingsWillingToBuy_Orbital(playerNegotiator);
            }
            else
            {
                return ColonyThingsWillingToBuy_Caravan(playerNegotiator);
            }
        }

        public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            if(def.orbital)
            {
                GiveSoldThingToTrader_Orbital(toGive, countToGive, playerNegotiator);
            }
            else
            {
                //This implementation works for both
                GiveSoldThingToTrader_Orbital(toGive, countToGive, playerNegotiator);
            }
        }

        public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            if(def.orbital)
            {
                GiveSoldThingToPlayer_Orbital(toGive, countToGive, playerNegotiator);
            }
            else
            {
                GiveSoldThingToPlayer_Caravan(toGive, countToGive, playerNegotiator);
            }
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return things;
        }

        private void InitCompany(Pawn negotiator)
        {
            map = negotiator.Map;
            incidentParmsMercenariesHired = new IncidentParms_MercenariesHired()
            {
                target = map
            };

            if(lastMercGenTime == 0 || lastMercGenTime + 900000 <= Find.TickManager.TicksGame)
            {
                things = new ThingOwner<Thing>(this);

                ThingSetMakerParams parms = new ThingSetMakerParams
                {
                    traderDef = TraderKind,
                    tile = negotiator.Map.Tile,
                    makingFaction = Faction
                };

                things.TryAddRangeOrTransfer(ThingSetMakerDefOf.TraderStock.root.Generate(parms));

                lastMercGenTime = Find.TickManager.TicksGame;
            }
        }

        private IEnumerable<Thing> ColonyThingsWillingToBuy_Caravan(Pawn playerNegotiator)
        {
            foreach(Thing item in playerNegotiator.Map.listerThings.GetAllThings(t => t.def == ThingDefOf.Silver && t.IsInValidStorage()))
            {
                yield return item;
            }
        }

        private IEnumerable<Thing> ColonyThingsWillingToBuy_Orbital(Pawn playerNegotiator)
        {
            foreach (Thing item in TradeUtility.AllLaunchableThingsForTrade(playerNegotiator.Map, this))
            {
                yield return item;
            }
        }

        private void GiveSoldThingToTrader_Orbital(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            Thing thing = toGive.SplitOff(countToGive);

            thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this);

            Thing thing2 = TradeUtility.ThingFromStockToMergeWith(this, thing);

            if (thing2 != null)
            {
                if (!thing2.TryAbsorbStack(thing, false))
                {
                    thing.Destroy();
                }
                return;
            }

            things.TryAdd(thing, false);
        }

        private void GiveSoldThingToPlayer_Caravan(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            Thing thing = toGive.SplitOff(countToGive);
            Pawn pawn = thing as Pawn;

            if(pawn != null)
            {
                pawn.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);

                //Mercs are not slaves but the PreTraded() method assumes they are
                pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.FreedFromSlavery);
            }
            else
            {
                thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);
            }

            //An incident will trigger in the PostClose() method of the Dialog_Mercenaries class
            incidentParmsMercenariesHired.mercenaries.Add(pawn);
        }

        private void GiveSoldThingToPlayer_Orbital(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            Thing thing = toGive.SplitOff(countToGive);
            Pawn pawn = thing as Pawn;

            if(pawn != null)
            {
                pawn.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);

                //Mercs are not slaves but the PreTraded() method assumes they are
                pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.FreedFromSlavery);
            }
            else
            {
                thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);
            }

            TradeUtility.SpawnDropPod(DropCellFinder.TradeDropSpot(playerNegotiator.Map), playerNegotiator.Map, thing);
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref lastMercGenTime, "lastMercGenTime", 0);
            Scribe_References.Look(ref faction, "faction");
            Scribe_Deep.Look(ref things, "things");
        }
    }
}