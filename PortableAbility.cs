using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace PortableAbility;

[UsedImplicitly]
[StaticConstructorOnStartup]
public class PortableAbility {
    static PortableAbility() {
        var harmony = new Harmony("Vortex.PortableAbility");
        harmony.PatchAll();
    }
}