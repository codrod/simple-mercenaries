using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace SimpleMercenaries.Core
{
    public class CompanyDef : Def, ICommunicable
    {
        public FactionDef factionDef = null;
        public bool useDropPods = true;
        public List<UnitDef> unitList = new List<UnitDef>();
        public List<RankDef> rankList = new List<RankDef>();
        public UnitDef startingUnit = new UnitDef();

        public CompanyDef() { }

        public static CompanyDef Named(string defName)
        {
            return DefDatabase<CompanyDef>.GetNamed(defName);
        }

        public static CompanyDef GetFactionArmy(Faction faction)
        {
            return DefDatabase<CompanyDef>.AllDefs.Where(army => army.factionDef.defName == faction.def.defName).First();
        }

        public RankDef GetPawnRank(Pawn pawn)
        {
            return rankList.Where(rank => rank.pawnKindDef.defName == pawn.kindDef.defName).FirstOrDefault(null);
        }

        public UnitDef CreateUnitOfPawns(IEnumerable<Pawn> pawns)
        {
            UnitDef unit = new UnitDef();

            foreach (Pawn pawn in pawns)
                unit.Add(GetPawnRank(pawn));

            return unit;
        }

        public UnitDef GetAllSoldiersInArmy()
        {
            return CreateUnitOfPawns(PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists);
        }

        public UnitDef GetAllSoldiersInMap(Map map)
        {
            return CreateUnitOfPawns(map.mapPawns.FreeColonistsAndPrisoners);
        }

        public IEnumerable<Thing> SendToMap(IEnumerable<Thing> things, Map map, IntVec3 centerCell)
        {
            if (useDropPods == true)
                DropPodUtility.DropThingsNear(centerCell, map, things);
            else
                MapUtilities.SpawnThingsInMap(map, centerCell, things);

            return things;
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
            Dialog_Negotiation dialog_Negotiation = new Dialog_Negotiation(negotiator, this, CompanyDialogMaker.DialogFor(negotiator, GetFaction()), true);
            dialog_Negotiation.soundAmbient = SoundDefOf.RadioComms_Ambience;
            Find.WindowStack.Add(dialog_Negotiation);
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
    }
}
