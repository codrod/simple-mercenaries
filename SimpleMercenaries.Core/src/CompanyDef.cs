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

        public List<PawnKindDef> pawnKindDefs = new List<PawnKindDef>();

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
