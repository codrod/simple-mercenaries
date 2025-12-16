using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Linq;

namespace SimpleMercenaries.Core
{
    public static class MercenaryGenerator
    {
        public static Pawn Generate(PawnGenerationRequest request)
        {
            Pawn pawn = null;
            bool isValid = false;

            while (!isValid)
            {
                pawn = Verse.PawnGenerator.GeneratePawn(request);

                //Sometimes backstories don't match the fixed backstories for reasons...
                if
                (
                    (request.KindDef.fixedChildBackstories.Any() && !request.KindDef.fixedChildBackstories.Any(b => b == pawn.story.Childhood)) ||
                    (request.KindDef.fixedAdultBackstories.Any() && !request.KindDef.fixedAdultBackstories.Any(b => b == pawn.story.Adulthood))
                )
                {
                    pawn.Destroy();
                    continue;
                }

                isValid = true;
            }

            //Mercs should not join the colony as "slaves"
            pawn.guest.joinStatus = JoinStatus.JoinAsColonist;

            return pawn;
        }

        public static PawnGenerationRequest GetGenerationRequest(PawnKindDef pawnKindDef)
        {
            PawnGenerationRequest request = new PawnGenerationRequest(pawnKindDef);

            request.MustBeCapableOfViolence = true;
            request.ForceGenerateNewPawn = true;

            //Note adults are aged >= 20 according to game logic
            request.AllowedDevelopmentalStages = DevelopmentalStage.Adult;
            request.ExcludeBiologicalAgeRange = new FloatRange(0f, 19f);

            //Don't want mercs to have relations with colonists
            request.CanGeneratePawnRelations = false;
            request.ColonistRelationChanceFactor = 0f;

            return request;
        }
    }
}