using JetBrains.Annotations;
using RimWorld;
using Verse;

// ReSharper disable InconsistentNaming

namespace BottledAbilities;

[UsedImplicitly]
public class IngestionOutcomeDoer_GiveBottledAbility : IngestionOutcomeDoer {
    public readonly AbilityDef abilityDef = null!;
    public readonly int charges = 1;

    protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount) {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (abilityDef is null) {
            Log.Error($"[BottledAbilities] Missing abilityDef in {ingested.def.defName}.");
            return;
        }

        var hediffDef = DefDatabase<HediffDef>.GetNamed("BottledAbilityStatus")!;

        var hediff = (Hediff_BottledAbility)HediffMaker.MakeHediff(hediffDef, pawn);
        hediff.AbilityDef = abilityDef;
        hediff.Charges = charges;

        pawn.health.AddHediff(hediff);
    }
}