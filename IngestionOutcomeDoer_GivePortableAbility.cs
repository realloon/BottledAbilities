using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace PortableAbility;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class IngestionOutcomeDoer_GivePortableAbility : IngestionOutcomeDoer {
    protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount) {
        var containerComp = ingested.TryGetComp<CompPortableAbility>();
        if (containerComp == null) return;

        var abilityDef = containerComp.Props.abilityDef;
        var charges = containerComp.Props.charges;
        var hediffDef = containerComp.Props.hediffDef;

        if (abilityDef == null || hediffDef == null) return;

        // 2. 检查小人是否已经有这个能力的 Hediff
        var existingHediff = pawn.health.hediffSet.hediffs
            .OfType<Hediff_PortableAbility>()
            .FirstOrDefault(h => h.AbilityDef == abilityDef);

        if (existingHediff != null) {
            // 已有 -> 增加次数
            existingHediff.Charges += charges;
            // 刷新显示
            existingHediff.Severity = 1.0f;
            Messages.Message(
                $"{pawn.LabelShort} recharged {abilityDef.label} ({existingHediff.Charges} uses total).", pawn,
                MessageTypeDefOf.PositiveEvent);
        } else {
            // 没有 -> 新增 Hediff
            var newHediff = (Hediff_PortableAbility)HediffMaker.MakeHediff(hediffDef, pawn);
            newHediff.AbilityDef = abilityDef;
            newHediff.Charges = charges;
            pawn.health.AddHediff(newHediff);
            Messages.Message($"{pawn.LabelShort} gained {abilityDef.label} for {charges} uses.", pawn,
                MessageTypeDefOf.PositiveEvent);
        }
    }
}