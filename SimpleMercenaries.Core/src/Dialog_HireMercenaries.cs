using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace SimpleMercenaries.Core
{
    public class Dialog_HireMercenaries : Dialog_Trade
    {
        private Company company = null;

        private static readonly FloatRange mercArrivalTime = new FloatRange(2f, 3f);

        public Dialog_HireMercenaries(Pawn playerNegotiator, ITrader trader, bool giftsOnly = false) : base(playerNegotiator, trader, giftsOnly)
        {
            company = trader as Company;
        }

        public override void PostClose()
        {
            base.PostClose();

            if(this.company.IncidentParmsMercenariesHired?.mercenaries?.Any() ?? false)
            {
                Find.LetterStack.ReceiveLetter("Mercenaries Hired", "The mercenaries you hired will arrive in a few days.", LetterDefOf.PositiveEvent);

                Find.Storyteller.incidentQueue.Add(
                    IncidentParms_MercenariesHired.GetDef(),
                    Find.TickManager.TicksGame + Mathf.RoundToInt(mercArrivalTime.RandomInRange * 60000f),
                    this.company.IncidentParmsMercenariesHired,
                    (int)mercArrivalTime.min
                );
            }
        }
    }
}