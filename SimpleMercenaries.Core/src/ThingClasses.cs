using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace RMC
{
    public class Building_MilitaryCommsConsole : RimWorld.Building_CommsConsole
    {
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn negotiator)
        {
            List<FloatMenuOption> menuOptions = new List<FloatMenuOption>();

            menuOptions.Add(new FloatMenuOption("Call "+ArmyDef.GetFactionArmy(this.Faction).label, delegate {this.GiveUseCommsJob(negotiator, this.Faction);},
                MenuOptionPriority.Default, null, null, 0f, null, null));

            return menuOptions;
        }

        public new void GiveUseCommsJob(Pawn negotiator, ICommunicable target)
        {
            Job job = new Job(DefDatabase<JobDef>.GetNamed("RMC_JobDef_UseMilitaryCommsConsole"), this);
            job.commTarget = target;
            negotiator.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
        }
    }
}
