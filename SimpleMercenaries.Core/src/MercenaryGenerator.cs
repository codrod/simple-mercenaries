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
            //Need to validate pawns after generation because they sometimes dont have the right backstories
            Pawn pawn = Verse.PawnGenerator.GeneratePawn(request);

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