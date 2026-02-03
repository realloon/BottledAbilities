using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

// ReSharper disable InconsistentNaming

namespace BottledAbilities.HarmonyPatches;

[HarmonyPatch(typeof(Ability), nameof(Ability.Activate), typeof(LocalTargetInfo), typeof(LocalTargetInfo))]
public static class Postfix_Ability_Activate {
    [UsedImplicitly]
    public static void Postfix(Ability __instance, LocalTargetInfo target) {
        var pawn = __instance.pawn;

        var managerHediff = pawn?.health.hediffSet.hediffs
            .OfType<Hediff_BottledAbility>()
            .FirstOrDefault(h => h.AbilityDef == __instance.def);

        if (managerHediff is null) return;

        managerHediff.Charges -= 1;

        if (managerHediff.Charges > 0) return;

        pawn?.health.RemoveHediff(managerHediff);
        Messages.Message($"{pawn?.LabelShort}'s {__instance.def.label} has been depleted.", pawn,
            MessageTypeDefOf.NeutralEvent);
    }
}