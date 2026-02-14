using JetBrains.Annotations;
using RimWorld;

namespace BottledAbilities.HarmonyPatches;

[HarmonyLib.HarmonyPatch(typeof(RimWorld.DefGenerator), nameof(RimWorld.DefGenerator.GenerateImpliedDefs_PreResolve))]
public static class Postfix_DefGenerator_GenerateImpliedDefs_PreResolve {
    [UsedImplicitly]
    public static void Postfix(bool hotReload = false) {
        DefGenerator.GenerateOrUpdate(hotReload);
    }
}
