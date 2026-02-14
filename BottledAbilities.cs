using HarmonyLib;

namespace BottledAbilities;

public static class BottledAbilities {
    private static readonly Lock PatchLock = new();
    private static bool _patched;

    public static void EnsurePatched() {
        lock (PatchLock) {
            if (_patched) return;

            var harmony = new Harmony("Vortex.BottledAbilities");
            harmony.PatchAll();
            _patched = true;
        }
    }
}