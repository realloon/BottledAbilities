using RimWorld;
using Verse;

namespace PortableAbility;

public class CompPortableAbility : ThingComp {
    public override string CompInspectStringExtra() {
        var doer = parent.def.ingestible.outcomeDoers.OfType<IngestionOutcomeDoer_GivePortableAbility>()
            .FirstOrDefault()!;

        return $"Contains: {doer.abilityDef.label} ({doer.charges} charges)";
    }

    public Gizmo? GetInventoryGizmoExtra(Pawn pawn) {
        var doer = parent.def.ingestible.outcomeDoers.OfType<IngestionOutcomeDoer_GivePortableAbility>()
            .FirstOrDefault()!;

        if (pawn.abilities.GetAbility(doer.abilityDef) is not null) return null;

        return new Command_Action {
            defaultLabel = $"Use {parent.LabelNoCount}",
            defaultDesc = $"Gain {doer.abilityDef.label} ({doer.charges} charges)",
            icon = parent.def.uiIcon,
            iconAngle = parent.def.uiIconAngle,
            iconOffset = parent.def.uiIconOffset,
            action = () => FoodUtility.IngestFromInventoryNow(pawn, parent)
        };
    }
}