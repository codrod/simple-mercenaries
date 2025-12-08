using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace RMC
{
    public static class MapUtilities
    {

        public static Predicate<IntVec3> IsSafeLandingAreaInMap(Map map)
        {
            return x => x.Walkable(map) && x.GetFirstBuilding(map) == null && !x.Roofed(map);
        }

        public static void SpawnThingsInMap(Map map, IntVec3 centerCell, IEnumerable<Thing> things)
        {
            IntVec3 cell = new IntVec3();

            foreach (Thing thing in things)
            {
                CellFinder.TryFindRandomSpawnCellForPawnNear(centerCell, map, out cell);
                GenSpawn.Spawn(thing, cell, map, WipeMode.Vanish);
            }

            return;
        }

        public static int DestroyThingsInMap(Map map, ThingDef thingDef, int count)
        {
            List<SlotGroup> allGroupsListForReading = map.haulDestinationManager.AllGroupsListForReading;

            if (count == 0) return 0;

            for (int i = 0; i < allGroupsListForReading.Count && count != 0; i++)
            {
                SlotGroup slotGroup = allGroupsListForReading[i];

                foreach (Thing current in slotGroup.HeldThings)
                {
                    Thing innerIfMinified = current.GetInnerIfMinified();

                    if (innerIfMinified.def.defName == thingDef.defName)
                    {
                        if (count >= innerIfMinified.stackCount)
                        {
                            count -= innerIfMinified.stackCount;
                            innerIfMinified.Destroy(DestroyMode.Vanish);
                        }
                        else
                        {
                            innerIfMinified.SplitOff(count).Destroy(DestroyMode.Vanish);
                            count = 0;
                        }
                    }

                    if (count == 0) break;
                }
            }

            return count;
        }
    }
}
