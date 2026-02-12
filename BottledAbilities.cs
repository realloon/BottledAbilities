using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace BottledAbilities;

[UsedImplicitly]
[StaticConstructorOnStartup]
public class BottledAbilities {
    private static bool _patched;

    public static void EnsurePatched() {
        if (_patched) return;
        _patched = true;

        var harmony = new Harmony("Vortex.BottledAbilities");
        harmony.PatchAll();
    }

    static BottledAbilities() {
        EnsurePatched();
    }
}
