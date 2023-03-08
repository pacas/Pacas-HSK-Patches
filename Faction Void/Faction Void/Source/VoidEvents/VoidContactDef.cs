using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VoidEvents
{
    public class VoidContactDef : Def
    {
        public PawnKindDef negotiatorDef;
        public PawnKindDef bodyguardDef;
        public int bodyguardCount;
        public float begSuccessChance;
        public List<IncidentDef> incidentsAfterDeclining;
        public int incidentsAfterDecliningInTicks;
        public List<GiftOption> giftOptions;

        public int netralPeriodAfterBeg;
        public int netralPeriodAfterRespectfulDecline;

        public string voidStartingText;
        public string begOptionText;
        public string disrespectOptionText;
        public string respectfulDeclineOptionText;

        public List<string> begOptionResponses;
        public List<string> disrespectOptionResponses;
        public List<string> respectfulDeclineOptionResponses;
    }
}