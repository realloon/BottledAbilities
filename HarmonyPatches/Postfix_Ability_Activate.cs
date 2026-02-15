using JetBrains.Annotations;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using BottledAbilities.Helpers;

// ReSharper disable InconsistentNaming

namespace BottledAbilities.HarmonyPatches;

[HarmonyPatch(typeof(Ability), nameof(Ability.Activate), typeof(LocalTargetInfo), typeof(LocalTargetInfo))]
public static class Postfix_Ability_Activate_LocalTarget {
    [UsedImplicitly]
    public static void Postfix(Ability __instance, bool __result) {
        if (!__result) return;
        AbilityChargeHelper.ConsumeCharge(__instance);
    }
}

[HarmonyPatch(typeof(Ability), nameof(Ability.Activate), typeof(GlobalTargetInfo))]
public static class Postfix_Ability_Activate_GlobalTarget {
    [UsedImplicitly]
    public static void Postfix(Ability __instance, bool __result) {
        if (!__result) return;
        AbilityChargeHelper.ConsumeCharge(__instance);
    }
}