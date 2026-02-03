using JetBrains.Annotations;
using Verse;

// ReSharper disable InconsistentNaming

namespace PortableAbility;

[UsedImplicitly]
public class CompProperties_PortableAbility : CompProperties {
    public CompProperties_PortableAbility() {
        compClass = typeof(CompPortableAbility);
    }
}