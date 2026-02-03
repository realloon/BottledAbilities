using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace PortableAbility;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class IngestionOutcomeDoer_GivePortableAbility : IngestionOutcomeDoer {
    public readonly AbilityDef abilityDef;
    public readonly int charges = 1;

    protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount) {
        var hediffDef = DefDatabase<HediffDef>.GetNamed("PortableAbilityStatus")!;

        var hediff = (Hediff_PortableAbility)HediffMaker.MakeHediff(hediffDef, pawn);
        hediff.AbilityDef = abilityDef;
        hediff.Charges = charges;

        pawn.health.AddHediff(hediff);
    }
}