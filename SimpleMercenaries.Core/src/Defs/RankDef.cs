using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace RMC
{
    public class RankDef : Def
    {
        public string title = null;
        public BackstoryDef childhood = null;
        public BackstoryDef adulthood = null;
        public PawnKindDef pawnKindDef = null;
        public TrainingDef trainingDef = null;
        public EquipmentDef equipmentDef = null;
        public ThingDef weapon = null;
        public bool destroyInventory = false;
        public float cost = 0f;
        public int spawnTime = 0;

        public RankDef()
        {
            return;
        }

        public static RankDef Named(string defName)
        {
            return DefDatabase<RankDef>.GetNamed(defName);
        }
    }
}
