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

namespace RMC
{
    public class IncidentParms_Deploy : IncidentParms, IExposable
    {
        public UnitDef reinforcements = new UnitDef();

        public new void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref reinforcements, "reinforcements");
        }
    }

    public class IncidentWorker_Deploy : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 arrivalCell = new IntVec3();
            ArmyDef armyDef = null;
            UnitDef reinforcements;

            if (parms.faction == null)
            {
                if (GenDate.DaysPassed == 0)
                    parms.faction = map.ParentFaction;
                else
                    return true;
            }

            armyDef = ArmyDef.GetFactionArmy(parms.faction);

            try
            {
                if (GenDate.DaysPassed == 0)
                    reinforcements = armyDef.startingUnit;
                else
                    reinforcements = ((IncidentParms_Deploy)parms).reinforcements;
            }
            catch (InvalidCastException)
            {
                Log.Error("RMC: Deployment incident has incorrect parameter type no unit will be spawned");
                return false;
            }

            if (armyDef.useDropPods == true || GenDate.DaysPassed == 0)
                RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(MapUtilities.IsSafeLandingAreaInMap(map), map, out arrivalCell);
            else
                RCellFinder.TryFindRandomPawnEntryCell(out arrivalCell, map, 1.0f);

            armyDef.SendToMap(reinforcements.Spawn().Cast<Thing>(), map, arrivalCell);

            if(GenDate.DaysPassed > 0)
                Find.LetterStack.ReceiveLetter(def.letterLabel, def.letterText, LetterDefOf.PositiveEvent, new TargetInfo(arrivalCell, map, false));

            return true;
        }
    }
}