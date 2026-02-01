using Verse;

namespace PortableAbility;

public class CompPortableAbility : ThingComp {
    public CompProperties_PortableAbility Props => (CompProperties_PortableAbility)props;

    public override string CompInspectStringExtra() {
        return $"Contains: {Props.abilityDef!.label} ({Props.charges} charges)";
    }
}