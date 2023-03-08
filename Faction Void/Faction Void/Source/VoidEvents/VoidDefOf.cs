using EncounterFramework;
using RimWorld;
using Verse;
using Verse.AI;

namespace VoidEvents
{
	[DefOf]
	public static class VoidDefOf
	{
		public static FactionDef RH_VOID;
		public static ThingDef RH_Nerotonin8B;
		public static PawnKindDef RH_VOID_Member;
		public static PawnKindDef RH_VOID_Bodyguard;
		public static PawnKindDef RH_VOID_Elite;
		public static LetterDef Void_ThreatBig;
		public static ThingDef Void_DefoliatorShipPart;
		public static HediffDef Void_SecronomControlChip;
		public static HediffDef Void_RapidHealing;
		public static JobDef UseSecronomControlChip;
		public static IncidentDef Void_PlanetKiller;
		public static ThingDef Void_PlanetaryKiller;
		public static WorldObjectDef Void_PlanetaryKillerSite;
		public static HediffDef Void_SpaceProtection;
		public static HediffDef Void_Painstopper;
		public static LocationDef RH_PlanetaryKillerSite;
		public static ThoughtDef Void_AteMutantCorpse;
		public static ThoughtDef Void_AteMutantMeat;
		public static DutyDef Void_MoveInPlace;
		public static JobDef Void_Contact;
		public static VoidContactDef VoidContact;
		public static PawnKindDef RH_DF2_Stalker;
		public static JobDef Void_SwallowTarget;
		public static VoidPrisonerReleaseDemandDef VoidPrisonerReleaseDemand;
		public static JobDef Void_ReleasePrisoner;
		public static PawnKindDef RH_VOID_Undying;
		public static FactionDef RH2_Nerotonin4_Horde;
    }

	[DefOf]
	public static class VoidDefOf2
	{
		public static SitePartDef Void_PlanetaryKillerSite;
		public static GameConditionDef Void_PlanetKiller;
        public static IncidentDef Void_Contact;
    }
}
