using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace RMC
{
    public class EquipmentDef : Def
    {
        public List<Equipment> equipmentList = new List<Equipment>();
        public ThingDef weapon = null;

        public EquipmentDef()
        {

        }

        public static EquipmentDef Named(string defName)
        {
            return DefDatabase<EquipmentDef>.GetNamed(defName);
        }

        public Pawn Equip(Pawn pawn)
        {
            Apparel apparel = null;

            pawn.apparel.DestroyAll();

            foreach (Equipment equipment in equipmentList)
            {
                if (equipment.stuff != null)
                    apparel = (Apparel)ThingMaker.MakeThing(equipment.thing, equipment.stuff);
                else
                    apparel = (Apparel)ThingMaker.MakeThing(equipment.thing);

                if(equipment.color.r != float.PositiveInfinity)
                    apparel.SetColor(equipment.color);

                pawn.apparel.Wear(apparel);
            }

            return pawn;
        }
    }

    public class Equipment
    {
        public ThingDef thing = null;
        public ThingDef stuff = null;
        public Color color = new Color(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    }
}
