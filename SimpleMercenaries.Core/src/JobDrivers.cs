using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RMC
{
    public class JobDriver_UseMilitaryCommsConsole : RimWorld.JobDriver_UseCommsConsole
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            List<Toil> toils = new List<Toil>();

            toils.Add(Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell));

            Toil openMenuToil = new Toil();

            openMenuToil.initAction = delegate
            {
                Dialog_Recruit dialog = new Dialog_Recruit(this.pawn, true);
                dialog.soundAmbient = SoundDefOf.RadioComms_Ambience;
                Find.WindowStack.Add(dialog);
            };

            openMenuToil.defaultCompleteMode = ToilCompleteMode.Instant;
            openMenuToil.FailOnDespawnedOrNull(TargetIndex.A);

            toils.Add(openMenuToil);

            return toils;
        } 
    }
}
