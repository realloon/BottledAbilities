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
        return "VortexBA_CompContains".Translate(doer.abilityDef.label, doer.charges).ToString();
    }

    public Gizmo? GetInventoryGizmoExtra(Pawn pawn) {
        var doer = CachedDoer;
        if (pawn.abilities.GetAbility(doer.abilityDef) is not null) return null;

        return new Command_Action {
            defaultLabel = "VortexBA_CompUse".Translate(parent.LabelNoCount).ToString(),
            defaultDesc = "VortexBA_CompGain".Translate(doer.abilityDef.label, doer.charges).ToString(),
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
