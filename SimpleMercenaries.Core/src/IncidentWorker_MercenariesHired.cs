using Verse;
using Verse.AI;
using Verse.AI.Group;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine.TextCore.Text;

namespace SimpleMercenaries.Core
{
    public class IncidentWorker_MercenariesHired : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms _parms)
        {
            try
            {
                IncidentParms_MercenariesHired parms = _parms as IncidentParms_MercenariesHired;
                Map map = (Map)parms.target;
                IntVec3 arrivalCell;
                
                if(!RCellFinder.TryFindRandomPawnEntryCell(out arrivalCell, map, 1.0f))
                    throw new Exception("Could not find a valid spawn position");

                SpawnThingsInMap(map, arrivalCell, parms.mercenaries.Cast<Thing>());

                Find.LetterStack.ReceiveLetter(def.letterLabel, def.letterText, LetterDefOf.PositiveEvent, new TargetInfo(arrivalCell, map, false));
            }
            catch(Exception ex)
            {
                Log.Error("Failed to spawn hired mercenaries : " + ex.Message);
                return false;
            }

            return true;
        }

        private static void SpawnThingsInMap(Map map, IntVec3 centerCell, IEnumerable<Thing> things)
        {
            IntVec3 cell = new IntVec3();

            foreach (Thing thing in things)
            {
                CellFinder.TryFindRandomSpawnCellForPawnNear(centerCell, map, out cell);
                GenSpawn.Spawn(thing, cell, map, WipeMode.Vanish);
            }

            return;
        }
    }

    public class IncidentParms_MercenariesHired : IncidentParms, IExposable
    {
        public List<Pawn> mercenaries = new List<Pawn>();

        public static IncidentDef GetDef()
        {
            return DefDatabase<IncidentDef>.AllDefs.Single(d => d.defName == "SMercs_IncidentDef_MercenariesHired");
        }

        public new void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Collections.Look(ref mercenaries, "mercenaries", LookMode.Deep);
        }
    }
}