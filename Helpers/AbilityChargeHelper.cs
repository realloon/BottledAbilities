using RimWorld;
using Verse;

namespace BottledAbilities.Helpers;

public static class AbilityChargeHelper {
    public static void ConsumeCharge(Ability ability) {
        var pawn = ability.pawn;

        var managerHediff = pawn?.health.hediffSet.hediffs
            .OfType<Hediff_BottledAbility>()
            .FirstOrDefault(h => h.AbilityDef == ability.def);

        if (managerHediff is null) return;

        managerHediff.Charges -= 1;

        if (managerHediff.Charges > 0) return;

        pawn?.health.RemoveHediff(managerHediff);
        Messages.Message(
            "VortexBA_AbilityDepletedMessage".Translate(pawn?.LabelShort ?? string.Empty, ability.def.label), pawn,
            MessageTypeDefOf.NeutralEvent);
    }
}