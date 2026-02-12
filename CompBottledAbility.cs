using RimWorld;
using Verse;

namespace BottledAbilities;

public class CompBottledAbility : ThingComp {
    private bool _doerResolved;

    private IngestionOutcomeDoer_GiveBottledAbility CachedDoer {
        get {
            if (_doerResolved) return field!;

            _doerResolved = true;
            field = parent.def.ingestible!.outcomeDoers
                .OfType<IngestionOutcomeDoer_GiveBottledAbility>()
                .First();

            return field;
        }
    }

    public override string CompInspectStringExtra() {
        var doer = CachedDoer;
        return $"Contains: {doer.abilityDef.label} ({doer.charges} charges)";
    }

    public Gizmo? GetInventoryGizmoExtra(Pawn pawn) {
        var doer = CachedDoer;
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

    public static void InjectDescriptionHyperlinks(ThingDef parentDef) {
        var doer = parentDef.ingestible!.outcomeDoers
            .OfType<IngestionOutcomeDoer_GiveBottledAbility>().First();

        parentDef.descriptionHyperlinks ??= [];
        parentDef.descriptionHyperlinks.Add(doer.abilityDef);
    }
}