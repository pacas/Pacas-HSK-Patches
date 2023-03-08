using Verse;

namespace VoidEvents
{
    public class PawnToResurrect : IExposable
    {
        public int tickToResurrect;
        public Pawn pawnToResurrect;
        public int tile;

        public PawnToResurrect()
        {

        }
        public PawnToResurrect(Pawn pawn, int tickToResurrect, int tile)
        {
            this.pawnToResurrect = pawn;
            this.tickToResurrect = tickToResurrect;
            this.tile = tile;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref tickToResurrect, "tickToResurrect");
            Scribe_Values.Look(ref tile, "tile");
            Scribe_References.Look(ref pawnToResurrect, "pawnToResurrect", saveDestroyedThings: true);
        }
    }
}