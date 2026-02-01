using JetBrains.Annotations;
using HarmonyLib;
using RimWorld;
using Verse;

// ReSharper disable InconsistentNaming

namespace PortableAbility.HarmonyPatches;

[HarmonyPatch(typeof(FloatMenuOptionProvider_Ingest), "GetSingleOptionFor")]
[UsedImplicitly]
public class Prefix_FloatMenuOptionProvider_Ingest {
    [HarmonyPostfix]
    [UsedImplicitly]
    private static void FilterExistingAbility(
        Thing clickedThing,
        FloatMenuContext context,
        ref FloatMenuOption __result) {
        if (context.FirstSelectedPawn == null) return;

        var portableAbilityComp = clickedThing.TryGetComp<CompPortableAbility>();

        var abilityDef = portableAbilityComp?.Props.abilityDef;
        if (abilityDef == null) return;

        if (context.FirstSelectedPawn.abilities.GetAbility(abilityDef) == null) return;

        string label = !clickedThing.def.ingestible.ingestCommandString.NullOrEmpty()
            ? clickedThing.def.ingestible.ingestCommandString.Formatted(clickedThing.LabelShort)
            : "ConsumeThing".Translate(clickedThing.LabelShort, clickedThing);

        __result = new FloatMenuOption(
            $"{label}: {context.FirstSelectedPawn.LabelShort} already has {abilityDef.label}",
            null
        );
    }
}