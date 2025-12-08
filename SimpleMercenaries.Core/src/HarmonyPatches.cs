using Verse;
using Verse.AI;
using Verse.AI.Group;
using System.Reflection;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HarmonyLib;

/*
 * Bugs:
 * 1) The pawn generator class only takes Factions not FactionDefs. Which causes a problem if you try to mix armies
 * because if a custom faction def is used it wont exist. So the default faction will be used instead.
 * 
 * 2) Generated back stories may contradict the forced (disallowed) traits/work tags if both back stories are not forced?
 * In general mixing forced back stories with random causes problems. It is better to just force both childhood and adulthood
 * instead of just one.
 * 
 * 3) ISC should not have relatives! I noticed that royal tribute collector had a guard with a relation to one of the spacemarines
 * Should see if there is a solution that problem but there might not be.
 * 
 * 4) You can recruit units faster by making multiple reinforcement requests rather than just one large request. Should probably
 * prevent that from happening.
 */

namespace SimpleMercenaries.Core
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            Log.Message("RMC: Started");

            var harmony = new Harmony("com.github.codrod.RMC");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //foreach(BackstoryDef backstoryDef in DefDatabase<BackstoryDef>.AllDefs)
           //     if(backstoryDef.isNewBackstory)
           //         BackstoryDatabase.AddBackstory(backstoryDef.NewBackstory());

            Log.Message("RMC: Loaded");
        }
    }
}

