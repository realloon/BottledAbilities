using JetBrains.Annotations;
using HarmonyLib;
using RimWorld;
using Verse;
using BottledAbilities.Helpers;

// ReSharper disable InconsistentNaming

namespace BottledAbilities.HarmonyPatches;

[UsedImplicitly]
[HarmonyPatch(typeof(FloatMenuOptionProvider_Ingest), "GetSingleOptionFor")]
public class Prefix_FloatMenuOptionProvider_Ingest {
    [UsedImplicitly]
    [HarmonyPostfix]
    private static void FilterExistingAbility(
        Thing clickedThing,
        FloatMenuContext context,
        ref FloatMenuOption __result) {
        if (context.FirstSelectedPawn is null) return;

        var doer = clickedThing.def.ingestible?.outcomeDoers?.OfType<IngestionOutcomeDoer_GiveBottledAbility>()
            .FirstOrDefault();

        var abilityDef = doer?.abilityDef;
        if (abilityDef is null) return;

        var pawn = context.FirstSelectedPawn;
        if (pawn.abilities.GetAbility(abilityDef, includeTemporary: true) is null) return;
        if (AbilitySupplementHelper.ShouldAllowCooldownSupplementIngestion(pawn, abilityDef)) return;

        var label = !clickedThing.def.ingestible!.ingestCommandString.NullOrEmpty()
            ? clickedThing.def.ingestible.ingestCommandString.Formatted(clickedThing.LabelShort)
            : "ConsumeThing".Translate(clickedThing.LabelShort, clickedThing);

        __result = new FloatMenuOption("VortexBA_FloatMenuAlreadyHasAbility"
            .Translate(label, pawn.LabelShort, abilityDef.label), null);
    }
}
