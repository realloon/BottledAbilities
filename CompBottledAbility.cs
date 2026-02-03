using RimWorld;
using Verse;

namespace BottledAbilities;

public class CompBottledAbility : ThingComp {
    public override string CompInspectStringExtra() {
        var doer = parent.def.ingestible.outcomeDoers.OfType<IngestionOutcomeDoer_GiveBottledAbility>()
            .FirstOrDefault()!;

        return $"Contains: {doer.abilityDef.label} ({doer.charges} charges)";
    }

    public Gizmo? GetInventoryGizmoExtra(Pawn pawn) {
        var doer = parent.def.ingestible.outcomeDoers.OfType<IngestionOutcomeDoer_GiveBottledAbility>()
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