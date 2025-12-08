using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace SimpleMercenaries.Core
{
    //Custom def for specify back stories because such a def does not exist in RimWorld Core
    public class BackstoryDef : Def
    {
        public string identifier = null;
        public string baseDesc = null;
        public string title = null;
        public string titleShort = null;
        public bool isNewBackstory = false;

        public BodyTypeDef bodyTypeDef = null;
        public int chronologicalAge = -1;

        public WorkTags requiredWorkTags;
        public WorkTags disabledWorkTags;

        public List<BackstoryTrait> forcedTraits = new List<BackstoryTrait>();
        public List<TraitDef> disallowedTraits = new List<TraitDef>();

        /*public Backstory NewBackstory()
        {
            Backstory backstory = new Backstory();

            backstory.identifier = identifier;
            backstory.baseDesc = baseDesc;

            backstory.title = title;
            backstory.titleFemale = title;
            backstory.titleShort = titleShort;
            backstory.titleShortFemale = titleShort;

            backstory.requiredWorkTags = requiredWorkTags;
            backstory.workDisables = disabledWorkTags;

            if (forcedTraits.Count > 0)
            {
                if (backstory.forcedTraits == null)
                    backstory.forcedTraits = new List<TraitEntry>();

                foreach (BackstoryTrait forcedTrait in forcedTraits)
                    backstory.forcedTraits.Add(new TraitEntry(forcedTrait.traitDef, forcedTrait.degree));
            }

            if (disallowedTraits.Count > 0)
            {
                if (backstory.disallowedTraits == null)
                    backstory.disallowedTraits = new List<TraitEntry>();

                foreach(TraitDef disallowedTrait in disallowedTraits)
                    backstory.disallowedTraits.Add(new TraitEntry(disallowedTrait, 0));
            }

            return backstory;
        }*/
    }

    //Need to use BackstoryTrait instead of TraitEntry or else the mod will
    //fail during initialization because the TraitDefs have not been loaded yet
    public class BackstoryTrait
    {
        public TraitDef traitDef = null;
        public int degree = 0;
    }
}
