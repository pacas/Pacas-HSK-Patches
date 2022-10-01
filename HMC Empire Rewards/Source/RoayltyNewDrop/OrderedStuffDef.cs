using System.Collections.Generic;
using Verse;

namespace RimWorld
{
    public class OrderedStuffDef : Def
    {
        public string typeOfDrop;
        public string typeOfItem;
        public string typeOfQuality;
        public string quality;
        public List<ThingDef> stuffList;
        public List<ThingDef> thingsToChoose;
        public List<ThingDefCountClass> ammunition;
    }
}