using Verse;

namespace PortableAbility;

public class CompPortableAbility : ThingComp {
    public override string CompInspectStringExtra() {
        var doer = parent.def.ingestible?.outcomeDoers?.OfType<IngestionOutcomeDoer_GivePortableAbility>()
            .FirstOrDefault();

        return doer?.abilityDef is not null
            ? $"Contains: {doer.abilityDef.label} ({doer.charges} charges)"
            : base.CompInspectStringExtra();
    }
}