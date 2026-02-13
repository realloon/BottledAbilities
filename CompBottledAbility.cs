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
        return "VortexBA_CompContains".Translate(doer.abilityDef.label, doer.charges);
    }

    public Gizmo? GetInventoryGizmoExtra(Pawn pawn) {
        var doer = CachedDoer;
        if (pawn.abilities.GetAbility(doer.abilityDef, includeTemporary: true) is not null) return null;

        return new Command_Action {
            defaultLabel = "VortexBA_CompUse".Translate(parent.LabelNoCount),
            defaultDesc = "VortexBA_CompGain".Translate(doer.abilityDef.label, doer.charges),
            icon = parent.def.uiIcon,
            iconAngle = parent.def.uiIconAngle,
            iconOffset = parent.def.uiIconOffset,
            action = () => FoodUtility.IngestFromInventoryNow(pawn, parent)
        };
    }

}
