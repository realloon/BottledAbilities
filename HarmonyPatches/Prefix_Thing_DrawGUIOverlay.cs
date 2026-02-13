using HarmonyLib;
using JetBrains.Annotations;
using Verse;

// ReSharper disable InconsistentNaming

namespace BottledAbilities.HarmonyPatches;

[UsedImplicitly]
[HarmonyPatch(typeof(Thing), nameof(Thing.DrawGUIOverlay))]
public static class Prefix_Thing_DrawGUIOverlay {
    [UsedImplicitly]
    [HarmonyPrefix]
    private static bool HideItemStackLabelsOnAbilityShelf(Thing __instance) {
        return !AbilityShelfVisibilityUtility.IsItemOnAbilityShelf(__instance);
    }
}
