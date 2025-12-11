using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace SimpleMercenaries.Core
{
    public class CompanyDef : Def, ICommunicable, ITrader, IThingHolder
    {
        // original
        public FactionDef factionDef = null;

        public List<PawnKindDef> pawnKindDefs = new List<PawnKindDef>();

        //ITrader

        public TraderKindDef traderKindDef = null;

        private ThingOwner things;

        private List<Pawn> soldPrisoners = new List<Pawn>();

        private int randomPriceFactorSeed = -1;

        private Map map = null;

        public TraderKindDef TraderKind
        {
            get
            {
                return traderKindDef;
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
                return GetFaction();
            }
        }

        public TradeCurrency TradeCurrency
        {
            get
            {
                return traderKindDef.tradeCurrency;
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

        public CompanyDef() { }

        public static CompanyDef Named(string defName)
        {
            return DefDatabase<CompanyDef>.GetNamed(defName);
        }

        public static CompanyDef GetCompanyByFaction(Faction faction)
        {
            return DefDatabase<CompanyDef>.AllDefs.Where(army => army.factionDef.defName == faction.def.defName).First();
        }

        public string GetCallLabel()
        {
            return label;
        }

        public string GetInfoText()
        {
            return label;
        }

        public void TryOpenComms(Pawn negotiator)
        {
            if (CanTradeNow)
            {
                map = negotiator.Map;
                things = new ThingOwner<Thing>(this);

                ThingSetMakerParams parms = new ThingSetMakerParams
                {
                    traderDef = TraderKind,
                    tile = negotiator.Map.Tile
                };
                things.TryAddRangeOrTransfer(ThingSetMakerDefOf.TraderStock.root.Generate(parms));

                Find.WindowStack.Add(new Dialog_Trade(negotiator, this));
                //LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.Critical);
                //PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(Goods.OfType<Pawn>(), "LetterRelatedPawnsTradeShip".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent);
                //TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.TradeGoodsMustBeNearBeacon);
            }
            
            /*Dialog_Negotiation dialog_Negotiation = new Dialog_Negotiation(negotiator, this, CompanyDialogMaker.DialogFor(negotiator, GetFaction()), true);
            dialog_Negotiation.soundAmbient = SoundDefOf.RadioComms_Ambience;
            Find.WindowStack.Add(dialog_Negotiation);*/
        }

        public Faction GetFaction()
        {
            return Find.FactionManager.AllFactions.Where(f => f.def.defName == this.factionDef.defName).Single();
        }

        public FloatMenuOption CommFloatMenuOption(Building_CommsConsole console, Pawn negotiator)
        {
            string text = "CallOnRadio".Translate(GetCallLabel());

            return FloatMenuUtility.DecoratePrioritizedTask(
                new FloatMenuOption(
                    text,
                    delegate{console.GiveUseCommsJob(negotiator, this);},
                    factionDef.FactionIcon,
                    GetFaction().Color,
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
    }
}
