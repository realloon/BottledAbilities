using JetBrains.Annotations;
using Verse;

// ReSharper disable InconsistentNaming

namespace BottledAbilities;

[UsedImplicitly]
public class CompProperties_BottledAbility : CompProperties {
    public CompProperties_BottledAbility() {
        compClass = typeof(CompBottledAbility);
    }
}