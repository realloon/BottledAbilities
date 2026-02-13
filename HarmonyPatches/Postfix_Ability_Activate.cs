using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using Verse;

// ReSharper disable InconsistentNaming

namespace BottledAbilities.HarmonyPatches;

[HarmonyPatch(typeof(Ability), nameof(Ability.Activate), typeof(LocalTargetInfo), typeof(LocalTargetInfo))]
public static class Postfix_Ability_Activate_LocalTarget {
    [UsedImplicitly]
    public static void Postfix(Ability __instance, bool __result) {
        if (!__result) return;
        BottledAbilityChargeUtility.ConsumeCharge(__instance);
    }
}

[HarmonyPatch(typeof(Ability), nameof(Ability.Activate), typeof(GlobalTargetInfo))]
public static class Postfix_Ability_Activate_GlobalTarget {
    [UsedImplicitly]
    public static void Postfix(Ability __instance, bool __result) {
        if (!__result) return;
        BottledAbilityChargeUtility.ConsumeCharge(__instance);
    }
}

internal static class BottledAbilityChargeUtility {
    public static void ConsumeCharge(Ability ability) {
        var pawn = ability.pawn;

        var managerHediff = pawn?.health.hediffSet.hediffs
            .OfType<Hediff_BottledAbility>()
            .FirstOrDefault(h => h.AbilityDef == ability.def);

        if (managerHediff is null) return;

        managerHediff.Charges -= 1;

        if (managerHediff.Charges > 0) return;

        pawn?.health.RemoveHediff(managerHediff);
        Messages.Message("VortexBA_AbilityDepletedMessage".Translate(pawn?.LabelShort ?? string.Empty, ability.def.label), pawn,
            MessageTypeDefOf.NeutralEvent);
    }
}
