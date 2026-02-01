using JetBrains.Annotations;
using RimWorld;
using Verse;

// ReSharper disable InconsistentNaming

namespace PortableAbility;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public class CompProperties_PortableAbility : CompProperties {
    [UsedImplicitly]
    public AbilityDef? abilityDef;

    [UsedImplicitly]
    public int charges = 1;

    [UsedImplicitly]
    public HediffDef? hediffDef; //  PortableAbilityStatus

    public CompProperties_PortableAbility() {
        compClass = typeof(CompPortableAbility);
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef) {
        foreach (var item in base.ConfigErrors(parentDef)) {
            yield return item;
        }

        if (abilityDef == null) {
            yield return "abilityDef is null";
        }

        if (hediffDef == null) {
            yield return "hediffDef is null";
        }
    }
}