using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace FactionTweaks
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        public static Harmony harmonyInstance;
        static HarmonyInit()
        {
            harmonyInstance = new Harmony("ChickenPlucker.FactionTweaks");
            harmonyInstance.PatchAll();
        }
    }

    [HarmonyPatch(typeof(FactionUIUtility))]
    [HarmonyPatch("DrawFactionRow")]
    internal class FactionUIUtility_DrawFactionRow
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            List<CodeInstruction> newInstructions = new List<CodeInstruction>();
            bool found = false;
            FieldInfo leaderInfo = AccessTools.Field(typeof(Faction), nameof(Faction.leader));
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!found && i > 1 && instructionList[i - 2].opcode == OpCodes.Ldfld && instructionList[i - 1].opcode == OpCodes.Ldfld && instructionList[i - 1].OperandIs(leaderInfo)
                    && instructionList[i].opcode == OpCodes.Brtrue_S)
                {
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldnull, null));
                    newInstructions.Add(new CodeInstruction(OpCodes.Ceq, null));
                    newInstructions.Add(new CodeInstruction(OpCodes.Not, null));
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newInstructions.Add(new CodeInstruction(OpCodes.Call, typeof(FactionUIUtility_DrawFactionRow).GetMethod("ShouldDisplayFactionLeader")));
                    newInstructions.Add(new CodeInstruction(OpCodes.And));
                    newInstructions.Add(new CodeInstruction(OpCodes.Brtrue_S, instructionList[i].operand));
                    found = true;
                }
                else
                {
                    newInstructions.Add(instructionList[i]);
                }
            }
            foreach (CodeInstruction ins in newInstructions)
            {
                yield return ins;
            }
            yield break;
        }

        public static bool ShouldDisplayFactionLeader(Faction faction)
        {
            if (faction != null)
            {
                FactionOptions options = faction.def?.GetModExtension<FactionOptions>();
                if (options != null)
                {
                    if (options.hideFactionLeader)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Faction))]
    [HarmonyPatch("TryGenerateNewLeader")]
    internal class TryGenerateNewLeader_Patch
    {
        public static void Postfix(Faction __instance, bool __result)
        {
            if (__result)
            {
                FactionOptions options = __instance.def.GetModExtension<FactionOptions>();
                if (options != null)
                {
                    if (options.leaderNames.Count > 0)
                    {
                        __instance.leader.Name = options.leaderNames.RandomElement();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(IdeoFoundation), "RandomizePrecepts")]
    public static class IdeoFoundation_RandomizePrecepts_Patch
    {
        public static bool Prefix(IdeoFoundation __instance, bool init, IdeoGenerationParms parms)
        {
            FactionOptions extension = parms.forFaction?.GetModExtension<FactionOptions>();
            if (extension?.preceptsToAdd != null)
            {
                __instance.ideo.ClearPrecepts();
                foreach (PreceptDef def in extension.preceptsToAdd)
                {
                    __instance.ideo.AddPrecept(PreceptMaker.MakePrecept(def));
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(IdeoUtility), "CanUseIdeo")]
    public static class IdeoUtility_CanUseIdeo_Patch
    {
        public static bool Prefix(FactionDef factionDef, Ideo ideo, List<PreceptDef> disallowedPrecepts)
        {
            IEnumerable<Faction> factions = Find.FactionManager.AllFactions.Where(x => x.ideos?.PrimaryIdeo == ideo);
            foreach (Faction faction in factions)
            {
                FactionOptions extension = faction.def.GetModExtension<FactionOptions>();
                if (extension != null && faction.def != factionDef && extension.hasUniqueIdeo)
                {
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Ideo), "SetIcon")]
    public static class Ideo_SetIcon_Patch
    {
        public static void Postfix(Ideo __instance)
        {
            IEnumerable<Faction> factions = Find.FactionManager.AllFactions.Where(x => x.ideos?.PrimaryIdeo == __instance);
            foreach (Faction faction in factions)
            {
                FactionOptions extension = faction.def.GetModExtension<FactionOptions>();
                if (extension != null)
                {
                    if (extension.customIdeoIcon != null)
                    {
                        __instance.iconDef = extension.customIdeoIcon;
                    }
                    if (extension.customIdeoColor != null)
                    {
                        __instance.colorDef = extension.customIdeoColor;
                    }
                    if (extension.customIdeoDescription.NullOrEmpty() is false)
                    {
                        __instance.description = extension.customIdeoDescription;
                    }
                }
            }
        }
    }
}
