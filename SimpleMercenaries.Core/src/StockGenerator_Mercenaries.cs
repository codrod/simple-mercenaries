using System;
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
        public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
        {
            int count = countRange.RandomInRange;

            Company company = CompanyManager.GetCompanyByFaction(faction);

            for (int i = 0; i < count; i++)
            {
                PawnKindDef kind = company.def.pawnKindDefs.RandomElementByWeight(pk => 1 / Math.Max(1, pk.combatPower));

                PawnGenerationRequest request = MercenaryGenerator.GetGenerationRequest(kind);

                request.Faction = faction;
                request.Tile = forTile;
                request.ForceAddFreeWarmLayerIfNeeded = !company.def.orbital;
                
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
