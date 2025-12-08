using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace RMC
{
    public class SkillRange
    {
        public SkillDef skillDef = null;

        public int min = 0;
        public int max = 0;
        public Passion passion = Passion.None;

        public int bonus = 0;

        public SkillRange()
        {
            return;
        }

        public Pawn SetLevel(Pawn pawn)
        {
            SkillRecord skillRecord;

            if (bonus != 0)
            {
                skillRecord = pawn.skills.GetSkill(skillDef);

                if (skillRecord.Level < bonus)
                    skillRecord.Level += bonus;
            }
            else
            {
                skillRecord = pawn.skills.GetSkill(skillDef);
                skillRecord.Level = Rand.RangeInclusive(min, max);
                skillRecord.passion = passion;
            }

            return pawn;
        }
    }

    public class TrainingDef : Def
    {
        public List<SkillRange> skillRangeList = new List<SkillRange>();

        public TrainingDef()
        {
            return;
        }

        public static TrainingDef Named(string defName)
        {
            return DefDatabase<TrainingDef>.GetNamed(defName);
        }

        public Pawn Train(Pawn pawn)
        {
            foreach (SkillRange skillRange in skillRangeList)
                skillRange.SetLevel(pawn);

            return pawn;
        }
    }
}
