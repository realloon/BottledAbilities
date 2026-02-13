using JetBrains.Annotations;
using Verse;

// ReSharper disable InconsistentNaming

namespace BottledAbilities;

[UsedImplicitly]
public class CompProperties_EmptyAbilityJar : CompProperties {
    public CompProperties_EmptyAbilityJar() {
        compClass = typeof(CompEmptyAbilityJar);
    }
}

