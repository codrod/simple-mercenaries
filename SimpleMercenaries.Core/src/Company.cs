using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace SimpleMercenaries.Core
{
    public class Company : ICommunicable, ITrader, IThingHolder, IExposable
    {
        public CompanyDef def = null;

        //ITrader
        private Faction faction = null;

        private int lastMercGenTime = 0;

        private ThingOwner things;

        private List<Pawn> soldPrisoners = new List<Pawn>();

        private int randomPriceFactorSeed = -1;

        private Map map = null;

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
                    Pawn pawn = things[i] as Pawn;
                    if (pawn == null || !soldPrisoners.Contains(pawn))
                    {
                        yield return things[i];
                    }
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

        //Endof Itrader

        public Company() {}

        public Company(CompanyDef def)
        {
            this.def = def;
            this.faction = Find.FactionManager.AllFactions.Single(f => f.def.defName == this.def.factionDef.defName);
        }

        public string GetCallLabel()
        {
            return def.label;
        }

        public string GetInfoText()
        {
            return def.label;
        }

        public void TryOpenComms(Pawn negotiator)
        {
            //Needs to be initialized but not sure the best place to do so...
            map = negotiator.Map;

            GenerateMercsForHire(negotiator);
            Find.WindowStack.Add(new Dialog_Trade(negotiator, this));

            //LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.Critical);
            //PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(Goods.OfType<Pawn>(), "LetterRelatedPawnsTradeShip".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent);
            //TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.TradeGoodsMustBeNearBeacon);

            /*Dialog_Negotiation dialog_Negotiation = new Dialog_Negotiation(negotiator, this, CompanyDialogMaker.DialogFor(negotiator, GetFaction()), true);
            dialog_Negotiation.soundAmbient = SoundDefOf.RadioComms_Ambience;
            Find.WindowStack.Add(dialog_Negotiation);*/
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
            foreach (Thing item in TradeUtility.AllLaunchableThingsForTrade(playerNegotiator.Map, this))
            {
                yield return item;
            }
            foreach (Pawn item2 in TradeUtility.AllSellableColonyPawns(playerNegotiator.Map, false))
            {
                yield return item2;
            }
        }

        public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
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
            Pawn pawn = thing as Pawn;
            if (pawn != null && pawn.RaceProps.Humanlike)
            {
                soldPrisoners.Add(pawn);
            }
            things.TryAdd(thing, false);
        }

        public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
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

            if (pawn != null)
            {
                soldPrisoners.Remove(pawn);
            }

            TradeUtility.SpawnDropPod(DropCellFinder.TradeDropSpot(playerNegotiator.Map), playerNegotiator.Map, thing);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return things;
        }

        private void GenerateMercsForHire(Pawn negotiator)
        {
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

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref lastMercGenTime, "lastMercGenTime", 0);
            Scribe_References.Look(ref faction, "faction");
            Scribe_Deep.Look(ref things, "things");
        }
    }
}