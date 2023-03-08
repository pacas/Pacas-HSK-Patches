using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;

namespace VoidEvents
{
	public class ShipPawnTypes : DefModExtension
	{
		public List<PawnType> pawnSpawnOptions;
	}
	public class PawnType
    {
		public int amount;
		public PawnKindDef pawnKindDef;
		public FactionDef factionDef;
    }
}
