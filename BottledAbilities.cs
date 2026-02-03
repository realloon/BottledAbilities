using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace BottledAbilities;

[UsedImplicitly]
[StaticConstructorOnStartup]
public class BottledAbilities {
    static BottledAbilities() {
        var harmony = new Harmony("Vortex.BottledAbilities");
        harmony.PatchAll();
    }
}