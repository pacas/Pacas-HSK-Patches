using System.Collections.Generic;
using Verse;

namespace VoidEvents
{
    public class CompProperties_SwallowedItems : CompProperties
    {
        public int maxSwallowedItems;
        public CompProperties_SwallowedItems()
        {
            compClass = typeof(CompSwallowedItems);
        }
    }
    public class CompSwallowedItems : ThingComp, IThingHolder
    {
        public ThingOwner innerContainer;
        public CompSwallowedItems()
        {
            innerContainer = new ThingOwner<Thing>(this);
        }
        public CompProperties_SwallowedItems Props => base.props as CompProperties_SwallowedItems;
        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public override void CompTick()
        {
            base.CompTick();
            innerContainer.ThingOwnerTick();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            if (previousMap != null)
            {
                ReleaseSwallowedItems(previousMap);
            }
        }
        public void ReleaseSwallowedItems(Map map)
        {
            foreach (var item in innerContainer)
            {
                innerContainer.Remove(item);
                GenPlace.TryPlaceThing(item, parent.Position, map, ThingPlaceMode.Near);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
        }
    }
}
