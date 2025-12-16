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

namespace SimpleMercenaries.Core
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = new Harmony("dev.codrod.SimpleMercenaries.Core");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(RimWorld.Building_CommsConsole), nameof(RimWorld.Building_CommsConsole.GetCommTargets))]
    class Patch__Building_CommsConsole__GetCommTargets
    {
        static IEnumerable<ICommunicable> Postfix(IEnumerable<ICommunicable> targets)
        {
            return targets.Concat(CompanyManager.GetAllCompanies());
        }
    }
}