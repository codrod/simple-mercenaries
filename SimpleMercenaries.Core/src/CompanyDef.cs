using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace SimpleMercenaries.Core
{
    public class CompanyDef : Def
    {
        public FactionDef factionDef = null;

        public TraderKindDef traderKindDef = null;

        public List<PawnKindDef> pawnKindDefs = new List<PawnKindDef>();

        public CompanyDef() { }

        public static CompanyDef Named(string defName)
        {
            return DefDatabase<CompanyDef>.GetNamed(defName);
        }
    }
}
