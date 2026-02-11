using HarmonyLib;
using JetBrains.Annotations;
using Verse;

// ReSharper disable InconsistentNaming

namespace BottledAbilities.HarmonyPatches;

[UsedImplicitly]
[HarmonyPatch(typeof(Thing), nameof(Thing.DrawGUIOverlay))]
public static class Prefix_Thing_DrawGUIOverlay {
    private const string AbilityShelfDefName = "VortexBA_AbilityShelfSmall";

    [UsedImplicitly]
    [HarmonyPrefix]
    private static bool HideItemStackLabelsOnAbilityShelf(Thing __instance) {
        if (__instance.def.category != ThingCategory.Item) return true;
        if (!__instance.Spawned || __instance.MapHeld is null) return true;

        var edifice = __instance.PositionHeld.GetEdifice(__instance.MapHeld);

        return edifice?.def.defName != AbilityShelfDefName;
    }
}