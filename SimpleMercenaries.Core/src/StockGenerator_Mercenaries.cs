using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace SimpleMercenaries.Core
{
    public class StockGenerator_Mercenaries : StockGenerator
    {
        private bool respectPopulationIntent;

        public PawnKindDef slaveKindDef;

        public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
        {
            int count = countRange.RandomInRange;

            Company company = CompanyManager.GetCompanyByFaction(faction);
            //CompanyDef company = CompanyDef.GetCompanyByFaction(faction);
            //CompanyDef company = DefDatabase<CompanyDef>.AllDefs.First();

            for (int i = 0; i < count; i++)
            {
                //remember to randomize this?
                PawnKindDef kind = company.def.pawnKindDefs.First();

                PawnGenerationRequest request = MercenaryGenerator.GetGenerationRequest(kind);

                request.Faction = faction;
                request.Tile = forTile;
                request.ForceAddFreeWarmLayerIfNeeded = !trader.orbital;
                
                Pawn pawn = MercenaryGenerator.Generate(request);
                
                yield return pawn;
            }
        }

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            if (thingDef.category == ThingCategory.Pawn && thingDef.race.Humanlike)
            {
                return thingDef.tradeability != Tradeability.None;
            }
            return false;
        }
    }
}
