using JetBrains.Annotations;
using HarmonyLib;
using RimWorld;
using Verse;

// ReSharper disable InconsistentNaming

namespace BottledAbilities.HarmonyPatches;

[UsedImplicitly]
[HarmonyPatch(typeof(ThingSelectionUtility), nameof(ThingSelectionUtility.SelectableByMapClick))]
public static class Postfix_ThingSelectionUtility_SelectableByMapClick {
    private const string AbilityShelfDefName = "VortexBA_AbilityShelfSmall";

    [UsedImplicitly]
    [HarmonyPostfix]
    private static void HideItemsOnAbilityShelf(Thing t, ref bool __result) {
        if (!__result) return;
        if (t.def.category != ThingCategory.Item) return;
        if (!t.Spawned || t.MapHeld is null) return;

        var edifice = t.PositionHeld.GetEdifice(t.MapHeld);
        if (edifice?.def.defName == AbilityShelfDefName) {
            __result = false;
        }
    }
}