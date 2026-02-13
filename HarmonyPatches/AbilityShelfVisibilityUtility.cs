using Verse;

namespace BottledAbilities.HarmonyPatches;

internal static class AbilityShelfVisibilityUtility {
    private const string AbilityShelfDefName = "VortexBA_AbilityShelfSmall";

    public static bool IsItemOnAbilityShelf(Thing thing) {
        if (thing.def.category != ThingCategory.Item) return false;
        if (!thing.Spawned || thing.MapHeld is null) return false;

        var edifice = thing.PositionHeld.GetEdifice(thing.MapHeld);
        return edifice?.def.defName == AbilityShelfDefName;
    }
}
