using JetBrains.Annotations;
using HarmonyLib;
using RimWorld;
using Verse;

// ReSharper disable InconsistentNaming

namespace BottledAbilities.HarmonyPatches;

[UsedImplicitly]
[HarmonyPatch(typeof(ThingSelectionUtility), nameof(ThingSelectionUtility.SelectableByMapClick))]
public static class Postfix_ThingSelectionUtility_SelectableByMapClick {
    [UsedImplicitly]
    [HarmonyPostfix]
    private static void HideItemsOnAbilityShelf(Thing t, ref bool __result) {
        if (!__result) return;
        if (AbilityShelfVisibilityUtility.IsItemOnAbilityShelf(t)) {
            __result = false;
        }
    }
}
