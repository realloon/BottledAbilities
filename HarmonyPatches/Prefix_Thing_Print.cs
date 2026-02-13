using JetBrains.Annotations;
using HarmonyLib;
using Verse;

// ReSharper disable InconsistentNaming

namespace BottledAbilities.HarmonyPatches;

[UsedImplicitly]
[HarmonyPatch(typeof(Thing), nameof(Thing.Print))]
public static class Prefix_Thing_Print {
    [UsedImplicitly]
    [HarmonyPrefix]
    private static bool HideItemsOnAbilityShelf(Thing __instance) {
        return !AbilityShelfVisibilityUtility.IsItemOnAbilityShelf(__instance);
    }
}
