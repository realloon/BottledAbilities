using JetBrains.Annotations;
using RimWorld;
using Verse;
using BottledAbilities.Helpers;

// ReSharper disable InconsistentNaming

namespace BottledAbilities;

[UsedImplicitly]
public class IngestionOutcomeDoer_GiveBottledAbility : IngestionOutcomeDoer {
    public AbilityDef abilityDef = null!;
    public int charges = 1;

    protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount) {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (abilityDef is null) {
            Log.Error($"[BottledAbilities] Missing abilityDef in {ingested.def.defName}.");
            return;
        }

        if (AbilitySupplementHelper.TryRefreshCooldownFromSupplement(pawn, abilityDef)) {
            return;
        }

        if (AbilitySupplementHelper.IsOwnedAbilityCooldownRefreshEnabled() &&
            AbilitySupplementHelper.HasOwnedPermanentAbility(pawn, abilityDef)) {
            return;
        }

        var hediffDef = DefDatabase<HediffDef>.GetNamed("VortexBA_ImbibedAbility")!;

        var hediff = (Hediff_BottledAbility)HediffMaker.MakeHediff(hediffDef, pawn);
        hediff.AbilityDef = abilityDef;
        hediff.Charges = charges;

        pawn.health.AddHediff(hediff);
    }
}
