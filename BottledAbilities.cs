using HarmonyLib;

namespace BottledAbilities;

public static class BottledAbilities {
    private static readonly Lazy<Harmony> HarmonyInstance = new(() => {
        var harmony = new Harmony("Vortex.BottledAbilities");
        harmony.PatchAll();
        return harmony;
    });

    public static void EnsurePatched() => _ = HarmonyInstance.Value;
}