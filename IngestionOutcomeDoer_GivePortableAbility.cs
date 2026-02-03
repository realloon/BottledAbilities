using JetBrains.Annotations;
using RimWorld;
using Verse;

// ReSharper disable InconsistentNaming

namespace PortableAbility;

[UsedImplicitly]
public class IngestionOutcomeDoer_GivePortableAbility : IngestionOutcomeDoer {
    public readonly AbilityDef abilityDef = null!;
    public readonly int charges = 1;

    protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount) {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (abilityDef is null) {
            Log.Error($"[PortableAbility] Missing abilityDef in {ingested.def.defName}.");
            return;
        }

        var hediffDef = DefDatabase<HediffDef>.GetNamed("PortableAbilityStatus")!;

        var hediff = (Hediff_PortableAbility)HediffMaker.MakeHediff(hediffDef, pawn);
        hediff.AbilityDef = abilityDef;
        hediff.Charges = charges;

        pawn.health.AddHediff(hediff);
    }
}