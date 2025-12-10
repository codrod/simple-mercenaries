using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Linq;

namespace SimpleMercenaries.Core
{
    public class MercenaryGenerator
    {
        Pawn pawn;
        TraitDef newTraitDef;
        Trait newTrait;
        //Backstory childhood;
        //Backstory adulthood;
        RankDef rank;

        public MercenaryGenerator(RankDef rank)
        {
            this.rank = rank;

            /*if (rank.childhood != null)
                BackstoryDatabase.TryGetWithIdentifier(rank.childhood.identifier, out childhood);

            if (rank.adulthood != null)
                BackstoryDatabase.TryGetWithIdentifier(rank.adulthood.identifier, out adulthood);*/
        }

        public Pawn Generate()
        {
            pawn = Verse.PawnGenerator.GeneratePawn(GetGenerationRequest());

            ForceFaction();
            ForceBackstory();

            if (rank.trainingDef != null) rank.trainingDef.Train(pawn);

            if (rank.destroyInventory)
            {
                pawn.inventory.DestroyAll();
                pawn.carryTracker.DestroyCarriedThing();
            }

            if (rank.weapon != null)
            {
                pawn.equipment.DestroyAllEquipment();
                pawn.equipment.AddEquipment((ThingWithComps)ThingMaker.MakeThing(rank.weapon));
            }

            if (rank.equipmentDef != null) rank.equipmentDef.Equip(pawn);

            return pawn;
        }

        PawnGenerationRequest GetGenerationRequest()
        {
            PawnGenerationRequest pawnGenerationRequest = new PawnGenerationRequest(rank.pawnKindDef);

            //This wont work if no faction exists with that factiondef
            pawnGenerationRequest.Faction = Find.World.factionManager.FirstFactionOfDef(rank.pawnKindDef.defaultFactionDef);

            return pawnGenerationRequest;
        }

        void ForceFaction()
        {
            (pawn as Thing).GetType().GetField("factionInt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pawn, Find.World.factionManager.OfPlayer);
        }

        void ForceBackstory()
        {
            /*if (childhood != null)
            {
                pawn.story.childhood = childhood;

                if (rank.childhood.bodyTypeDef != null)
                    pawn.story.bodyType = rank.childhood.bodyTypeDef;
            }

            if (adulthood != null)
            {
                pawn.story.adulthood = adulthood;

                if (rank.adulthood.bodyTypeDef != null)
                    pawn.story.bodyType = rank.adulthood.bodyTypeDef;
            }

            if (childhood != null || adulthood != null)
               ForceTraits();*/

            if (rank.title != null)
            {
                NameTriple oldName = (NameTriple)pawn.Name;
                NameTriple newName = new NameTriple(oldName.First, rank.title + " " + oldName.Last, oldName.Last);
                pawn.Name = newName;
            }

            int chronologicalAge = Math.Max(rank.childhood?.chronologicalAge ?? -1, rank.adulthood?.chronologicalAge ?? -1);

            if (chronologicalAge > -1)
            {
                try
                {
                    pawn.ageTracker.AgeChronologicalTicks = chronologicalAge * 3600000L;
                }
                catch (OverflowException)
                {
                    Log.Error($"RMC: Chronological age (in ticks) for pawn with rank '{rank.defName}' caused overflow: {chronologicalAge}");
                }
            }

            return;
        }

        void ForceTraits()
        {
            //Clear all traits
            pawn.story.traits.allTraits = new List<Trait>();

            /*if (childhood != null && childhood.forcedTraits != null)
                childhood.forcedTraits.ForEach(entry => pawn.story.traits.GainTrait(new Trait(entry.def, entry.degree)));

            if (adulthood != null && adulthood.forcedTraits != null)
                adulthood.forcedTraits.ForEach(entry => pawn.story.traits.GainTrait(new Trait(entry.def, entry.degree)));*/

            //Add more random traits if there is room for more traits
            ForceRandomTraits();
        
            //Without this Pawns can still do disabled work
            pawn.workSettings.EnableAndInitialize();

            //Removes passion symbols for disabled skills in the Bio tab
            pawn.skills.skills.Where(skill => skill.TotallyDisabled).ToList().ForEach(skill => { skill.Level = 0; skill.passion = Passion.None; });

            return;
        }

        void ForceRandomTraits()
        {
            int moreTraitsRequired = Rand.RangeInclusive(1, 3) - pawn.story.traits.allTraits.Count;

            while (moreTraitsRequired > 0)
            {
                newTraitDef = DefDatabase<TraitDef>.AllDefsListForReading.RandomElementByWeight((TraitDef tr) => tr.GetGenderSpecificCommonality(pawn.gender));
                newTrait = new Trait(newTraitDef, Verse.PawnGenerator.RandomTraitDegree(newTraitDef), true);

                if (TraitIsValid())
                {
                    pawn.story.traits.GainTrait(newTrait);
                    moreTraitsRequired--;
                }
            }
        }

        bool TraitIsValid()
        {
            /*if (WorkTagsConflict(childhood) || WorkTagsConflict(adulthood) || TraitIsDisallowed(childhood) || TraitIsDisallowed(adulthood) || TraitConflictsWithExistingTraits())
                return false;*/

            return true;
        }

        /*bool WorkTagsConflict(Backstory backstory)
        {
            if (
                backstory != null && (
                    (backstory.requiredWorkTags & newTrait.def.disabledWorkTags) != 0 ||
                    (backstory.workDisables & newTrait.def.requiredWorkTags) != 0
                )
            )
                return true;

            return false;
        }*/

        /*bool TraitIsDisallowed(Backstory backstory)
        {
            if (backstory != null && backstory.disallowedTraits != null)
                return backstory.disallowedTraits.Any(entry => entry.def.defName == newTraitDef.defName);

            return false;
        }*/

        bool TraitConflictsWithExistingTraits()
        {
            foreach (Trait curTrait in pawn.story.traits.allTraits)
            {
                if (pawn.story.traits.HasTrait(newTrait.def) || (curTrait.def.requiredWorkTags & newTrait.def.disabledWorkTags) != 0)
                    return true;

                if (curTrait.def.conflictingTraits != null && curTrait.def.conflictingTraits.Any(trait => trait.defName == newTrait.def.defName))
                    return false;
            }

            return false;
        }
    }
}