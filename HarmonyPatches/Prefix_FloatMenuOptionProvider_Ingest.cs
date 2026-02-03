using JetBrains.Annotations;
using HarmonyLib;
using RimWorld;
using Verse;

// ReSharper disable InconsistentNaming

namespace PortableAbility.HarmonyPatches;

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

        var doer = clickedThing.def.ingestible?.outcomeDoers?.OfType<IngestionOutcomeDoer_GivePortableAbility>()
            .FirstOrDefault();

        var abilityDef = doer?.abilityDef;
        if (abilityDef is null) return;

        if (context.FirstSelectedPawn.abilities.GetAbility(abilityDef) is null) return;

        var label = !clickedThing.def.ingestible!.ingestCommandString.NullOrEmpty()
            ? clickedThing.def.ingestible.ingestCommandString.Formatted(clickedThing.LabelShort)
            : "ConsumeThing".Translate(clickedThing.LabelShort, clickedThing);

        __result = new FloatMenuOption(
            $"{label}: {context.FirstSelectedPawn.LabelShort} already has {abilityDef.label}",
            null
        );
    }
}