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
            /*if (respectPopulationIntent && Rand.Value > StorytellerUtilityPopulation.PopulationIntent)
            {
                yield break;
            }*/

            /*if (faction != null && faction.ideos != null)
            {
                bool flag = true;
                foreach (Ideo allIdeo in faction.ideos.AllIdeos)
                {
                    if (!allIdeo.IdeoApprovesOfSlavery())
                    {
                        flag = false;
                        break;
                    }
                }
                if (!flag)
                {
                    yield break;
                }
            }*/

            int count = countRange.RandomInRange;
            
            /*for (int i = 0; i < count; i++)
            {
                Faction result;
                if (!Find.FactionManager.AllFactionsVisible.Where((Faction fac) => fac != Faction.OfPlayer && fac.def.humanlikeFaction && !fac.temporary).TryRandomElement(out result))
                {
                    break;
                }
                DevelopmentalStage developmentalStage = (Find.Storyteller.difficulty.ChildrenAllowed ? (DevelopmentalStage.Child | DevelopmentalStage.Adult) : DevelopmentalStage.Adult);
                PawnKindDef kind = ((slaveKindDef != null) ? slaveKindDef : PawnKindDefOf.Slave);
                Faction faction2 = result;
                PlanetTile? tile = forTile;
                bool forceAddFreeWarmLayerIfNeeded = !trader.orbital;
                DevelopmentalStage developmentalStages = developmentalStage;
                PawnGenerationRequest request = new PawnGenerationRequest(kind, faction2, PawnGenerationContext.NonPlayer, tile, false, false, false, true, false, 1f, forceAddFreeWarmLayerIfNeeded, true, false, true, true, false, false, false, false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, false, false, false, false, null, null, null, null, null, 0f, developmentalStages);
                yield return PawnGenerator.GeneratePawn(request);
            }*/

            PawnGroupMakerParms parms = new PawnGroupMakerParms();
            PawnGroupMaker groupMaker = faction.def.pawnGroupMakers.Where(pg => pg.kindDef == PawnGroupKindDefOf.Combat).First();

            for (int i = 0; i < count; i++)
            {
                PawnGenOption result;
                groupMaker.options.TryRandomElementByWeight((PawnGenOption gr) => gr.selectionWeight, out result);

                PawnKindDef kind = result.kind;
                Faction faction2 = parms.faction;
                DevelopmentalStage developmentalStages = DevelopmentalStage.Adult;
                PlanetTile? tile = forTile;
                bool forceAddFreeWarmLayerIfNeeded = !trader.orbital;

                PawnGenerationRequest request = new PawnGenerationRequest(kind, faction2, PawnGenerationContext.NonPlayer, tile, false, false, false, true, false, 1f, forceAddFreeWarmLayerIfNeeded, true, false, true, true, false, false, false, false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, false, false, false, false, null, null, null, null, null, 0f, developmentalStages)
                {
                    ForceRecruitable = true
                };

                Pawn pawn = PawnGenerator.GeneratePawn(request);
                pawn.kindDef = PawnKindDefOf.Slave;
                pawn.guest.joinStatus = JoinStatus.JoinAsColonist;

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
