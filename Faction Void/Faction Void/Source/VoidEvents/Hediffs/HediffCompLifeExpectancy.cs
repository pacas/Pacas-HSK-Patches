using Verse;

namespace VoidEvents
{
    public class HediffCompProperties_LifeExpectancy : HediffCompProperties
    {
        public float lifeExpectancy;
        public HediffCompProperties_LifeExpectancy()
        {
            compClass = typeof(HediffCompLifeExpectancy);
        }
    }

    public class HediffCompLifeExpectancy : HediffComp
    {
        public HediffCompProperties_LifeExpectancy Props => base.props as HediffCompProperties_LifeExpectancy;
        public float LifeExpectancy => Props.lifeExpectancy;
    }
}