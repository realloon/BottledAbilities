using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace PortableAbility;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class IngestionOutcomeDoer_GivePortableAbility : IngestionOutcomeDoer {
    protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount) {
        var containerComp = ingested.TryGetComp<CompPortableAbility>();
        if (containerComp is null) return;

        var abilityDef = containerComp.Props.abilityDef;
        var charges = containerComp.Props.charges;
        var hediffDef = DefDatabase<HediffDef>.GetNamed("PortableAbilityStatus");

        if (abilityDef is null || hediffDef is null) return;

        var newHediff = (Hediff_PortableAbility)HediffMaker.MakeHediff(hediffDef, pawn);
        newHediff.AbilityDef = abilityDef;
        newHediff.Charges = charges;
        pawn.health.AddHediff(newHediff);
    }
}