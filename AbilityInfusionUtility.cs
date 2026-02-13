using HarmonyLib;
using RimWorld;
using Verse;

namespace BottledAbilities;

internal static class AbilityInfusionUtility {
    public static bool TryInfuse(Pawn pawn, Ability ability, Thing emptyJar) {
        if (pawn is null || ability is null || emptyJar is null) return false;
        if (ability.pawn != pawn) return false;

        var settings = BottledAbilitiesMod.Settings;
        settings.InitializeIfNeeded();

        var def = ability.def;
        if (def is null) return Fail(pawn, "VortexBA_MessageInfuseFailed".Translate("VortexBA_MissingAbilityDef".Translate()));
        var abilityLabel = def.label.NullOrEmpty() ? GenText.SplitCamelCase(def.defName) : def.label;

        if (!settings.IsEnabled(def.defName)) {
            return Fail(pawn, "VortexBA_MessageInfuseFailed".Translate("VortexBA_AbilityDisabledInSettings".Translate(abilityLabel)));
        }

        // Exclude abilities that are temporarily granted via bottled abilities (Hediff_BottledAbility).
        if (pawn.health?.hediffSet?.hediffs?.OfType<Hediff_BottledAbility>()
                .Any(h => h.AbilityDef == def) == true) {
            return Fail(pawn, "VortexBA_MessageInfuseFailed".Translate("VortexBA_AbilityIsTemporary".Translate(abilityLabel)));
        }

        var jarDefName = $"VortexBA_{def.defName}";
        var jarDef = DefDatabase<ThingDef>.GetNamedSilentFail(jarDefName);
        if (jarDef is null) {
            return Fail(pawn, "VortexBA_MessageInfuseFailed".Translate("VortexBA_MissingJarDef".Translate(jarDefName)));
        }

        if (ability.GizmoDisabled(out var reason)) {
            reason ??= "VortexBA_AbilityNotCastable".Translate(abilityLabel);
            return Fail(pawn, "VortexBA_MessageInfuseFailed".Translate(reason));
        }

        // Reflection lookup first so we don't spend resources if something is seriously wrong.
        var preActivate = AccessTools.Method(ability.GetType(), "PreActivate");
        if (preActivate is null) {
            Log.ErrorOnce("[BottledAbilities] Unable to find Ability.PreActivate via reflection.", 1960248311);
            return Fail(pawn, "VortexBA_MessageInfuseFailed".Translate("VortexBA_PreActivateFailed".Translate()));
        }

        // Deduct resource costs (subset): Psyfocus/Entropy, Hemogen.
        if (def.IsPsycast) {
            if (pawn.psychicEntropy is null) {
                return Fail(pawn, "VortexBA_MessageInfuseFailed".Translate("VortexBA_MissingPsychicEntropy".Translate()));
            }

            if (def.EntropyGain > float.Epsilon && !pawn.psychicEntropy.TryAddEntropy(def.EntropyGain)) {
                return Fail(pawn, "VortexBA_MessageInfuseFailed".Translate("VortexBA_NotEnoughEntropyRoom".Translate()));
            }

            var psyfocusCost = ability.FinalPsyfocusCost(new LocalTargetInfo(pawn));
            if (psyfocusCost > float.Epsilon) {
                pawn.psychicEntropy.OffsetPsyfocusDirectly(0f - psyfocusCost);
            }
        }

        var hemogenCost = ability.HemogenCost();
        if (hemogenCost > float.Epsilon) {
            var hemogenGene = pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
            if (hemogenGene is null || hemogenGene.Value < hemogenCost) {
                return Fail(pawn, "VortexBA_MessageInfuseFailed".Translate("VortexBA_NotEnoughHemogen".Translate()));
            }

            GeneUtility.OffsetHemogen(pawn, 0f - hemogenCost);
        }

        // Trigger cooldown / internal charges bookkeeping without applying effects.
        if (!InvokePreActivate(preActivate, ability, emptyJar)) {
            return Fail(pawn, "VortexBA_MessageInfuseFailed".Translate("VortexBA_PreActivateFailed".Translate()));
        }

        var map = emptyJar.Map;
        var pos = emptyJar.Position;
        var wasForbidden = emptyJar.IsForbidden(Faction.OfPlayer);

        ConsumeOne(emptyJar);

        var jar = ThingMaker.MakeThing(jarDef);
        if (!GenPlace.TryPlaceThing(jar, pos, map, ThingPlaceMode.Near, out var placed)) {
            // Fallback: drop near the pawn.
            if (pawn.MapHeld != null) {
                GenPlace.TryPlaceThing(jar, pawn.PositionHeld, pawn.MapHeld, ThingPlaceMode.Near, out placed);
            }
        }

        if (placed is not null && wasForbidden) {
            placed.SetForbidden(true, warnOnFail: false);
        }

        Messages.Message("VortexBA_MessageInfused".Translate(pawn.LabelShort, abilityLabel), pawn, MessageTypeDefOf.PositiveEvent);
        return true;
    }

    private static bool InvokePreActivate(System.Reflection.MethodInfo preActivate, Ability ability, Thing targetThing) {
        try {
            preActivate.Invoke(ability, [new LocalTargetInfo?(new LocalTargetInfo(targetThing))]);
            return true;
        } catch (Exception e) {
            Log.Error($"[BottledAbilities] Failed to invoke PreActivate: {e}");
            return false;
        }
    }

    private static void ConsumeOne(Thing emptyJar) {
        if (emptyJar.stackCount <= 1) {
            emptyJar.Destroy(DestroyMode.Vanish);
            return;
        }

        var consumed = emptyJar.SplitOff(1);
        consumed.Destroy(DestroyMode.Vanish);
    }

    private static bool Fail(Pawn pawn, TaggedString reason) {
        Messages.Message(reason, pawn, MessageTypeDefOf.RejectInput);
        return false;
    }
}
