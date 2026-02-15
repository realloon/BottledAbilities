using BottledAbilities.Helpers;
using JetBrains.Annotations;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

// ReSharper disable InconsistentNaming

namespace BottledAbilities.HarmonyPatches;

[HarmonyPatch(typeof(Ability), "get_CanCast")]
public static class Postfix_Ability_get_CanCast {
    [UsedImplicitly]
    public static void Postfix(Ability __instance, ref AcceptanceReport __result) {
        if (AbilityCastConditionHelper.ShouldIgnoreCastConditions(__instance)) {
            __result = true;
        }
    }
}

[HarmonyPatch(typeof(Psycast), "get_CanCast")]
public static class Postfix_Psycast_get_CanCast {
    [UsedImplicitly]
    public static void Postfix(Psycast __instance, ref AcceptanceReport __result) {
        if (AbilityCastConditionHelper.ShouldIgnoreCastConditions(__instance)) {
            __result = true;
        }
    }
}

[HarmonyPatch(typeof(Ability), nameof(Ability.GizmoDisabled))]
public static class Postfix_Ability_GizmoDisabled {
    [UsedImplicitly]
    public static void Postfix(Ability __instance, ref bool __result, ref string? reason) {
        if (!AbilityCastConditionHelper.ShouldIgnoreCastConditions(__instance)) {
            return;
        }

        __result = false;
        reason = null;
    }
}

[HarmonyPatch(typeof(Psycast), nameof(Psycast.GizmoDisabled))]
public static class Postfix_Psycast_GizmoDisabled {
    [UsedImplicitly]
    public static void Postfix(Psycast __instance, ref bool __result, ref string? reason) {
        if (!AbilityCastConditionHelper.ShouldIgnoreCastConditions(__instance)) {
            return;
        }

        __result = false;
        reason = null;
    }
}

[HarmonyPatch(typeof(Psycast), nameof(Psycast.Activate), typeof(LocalTargetInfo), typeof(LocalTargetInfo))]
public static class Prefix_Psycast_Activate_LocalTarget {
    private delegate bool
        ActivateLocalNonVirtualDelegate(Ability ability, LocalTargetInfo target, LocalTargetInfo dest);

    private static readonly ActivateLocalNonVirtualDelegate ActivateLocalNonVirtual =
        AccessTools.MethodDelegate<ActivateLocalNonVirtualDelegate>(
            AccessTools.Method(typeof(Ability), nameof(Ability.Activate),
                [typeof(LocalTargetInfo), typeof(LocalTargetInfo)])!,
            virtualCall: false);

    [UsedImplicitly]
    public static bool Prefix(Psycast __instance, LocalTargetInfo target, LocalTargetInfo dest, ref bool __result) {
        if (!AbilityCastConditionHelper.ShouldIgnoreCastConditions(__instance)) {
            return true;
        }

        if (!ModLister.CheckRoyalty("Psycast") || __instance.def.EntropyGain > float.Epsilon &&
            !__instance.pawn.psychicEntropy.TryAddEntropy(__instance.def.EntropyGain, overLimit: true)) {
            __result = false;
            return false;
        }

        var psyfocusCost = __instance.FinalPsyfocusCost(target);
        if (psyfocusCost > float.Epsilon) {
            __instance.pawn.psychicEntropy.OffsetPsyfocusDirectly(0f - psyfocusCost);
        }

        if (__instance.def.showPsycastEffects) {
            if (__instance.EffectComps.Any(comp => comp.Props.psychic)) {
                if (__instance.def.HasAreaOfEffect) {
                    FleckMaker.Static(target.Cell, __instance.pawn.Map, FleckDefOf.PsycastAreaEffect,
                        __instance.def.EffectRadius);
                    SoundDefOf.PsycastPsychicPulse.PlayOneShot(new TargetInfo(target.Cell, __instance.pawn.Map));
                } else {
                    SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(target.Cell, __instance.pawn.Map));
                }
            } else if (__instance.def.HasAreaOfEffect && __instance.def.canUseAoeToGetTargets) {
                SoundDefOf.Psycast_Skip_Pulse.PlayOneShot(new TargetInfo(target.Cell, __instance.pawn.Map));
            }
        }

        __result = ActivateLocalNonVirtual(__instance, target, dest);
        return false;
    }
}

[HarmonyPatch(typeof(Psycast), nameof(Psycast.Activate), typeof(GlobalTargetInfo))]
public static class Prefix_Psycast_Activate_GlobalTarget {
    private delegate bool ActivateGlobalNonVirtualDelegate(Ability ability, GlobalTargetInfo target);

    private static readonly ActivateGlobalNonVirtualDelegate ActivateGlobalNonVirtual =
        AccessTools.MethodDelegate<ActivateGlobalNonVirtualDelegate>(
            AccessTools.Method(typeof(Ability), nameof(Ability.Activate), [typeof(GlobalTargetInfo)])!,
            virtualCall: false);

    [UsedImplicitly]
    public static bool Prefix(Psycast __instance, GlobalTargetInfo target, ref bool __result) {
        if (!AbilityCastConditionHelper.ShouldIgnoreCastConditions(__instance)) {
            return true;
        }

        if (__instance.def.EntropyGain > float.Epsilon &&
            !__instance.pawn.psychicEntropy.TryAddEntropy(__instance.def.EntropyGain, overLimit: true)) {
            __result = false;
            return false;
        }

        var psyfocusCost = __instance.def.PsyfocusCost;
        if (psyfocusCost > float.Epsilon) {
            __instance.pawn.psychicEntropy.OffsetPsyfocusDirectly(0f - psyfocusCost);
        }

        __result = ActivateGlobalNonVirtual(__instance, target);
        return false;
    }
}

[HarmonyPatch(typeof(Verb_CastPsycast), nameof(Verb_CastPsycast.ValidateTarget))]
public static class Prefix_Verb_CastPsycast_ValidateTarget {
    private delegate bool BaseValidateTargetDelegate(Verb_CastAbility verb, LocalTargetInfo target, bool showMessages);

    private static readonly BaseValidateTargetDelegate BaseValidateTarget =
        AccessTools.MethodDelegate<BaseValidateTargetDelegate>(
            AccessTools.Method(typeof(Verb_CastAbility), nameof(Verb_CastAbility.ValidateTarget),
                [typeof(LocalTargetInfo), typeof(bool)])!,
            virtualCall: false);

    [UsedImplicitly]
    public static bool Prefix(Verb_CastPsycast __instance, LocalTargetInfo target, bool showMessages,
        ref bool __result) {
        var psycast = __instance.Psycast;
        if (psycast is null || !AbilityCastConditionHelper.ShouldIgnoreCastConditions(psycast)) {
            return true;
        }

        if (!BaseValidateTarget(__instance, target, showMessages)) {
            __result = false;
            return false;
        }

        var canTargetBosses = __instance.ability.EffectComps.All(effect => effect.Props.canTargetBosses);
        var targetPawn = target.Pawn;
        if (!canTargetBosses && targetPawn != null && targetPawn.kindDef.isBoss) {
            if (showMessages) {
                Messages.Message("CommandPsycastInsanityImmune".Translate(), __instance.caster,
                    MessageTypeDefOf.RejectInput,
                    historical: false);
            }

            __result = false;
            return false;
        }

        __result = true;
        return false;
    }
}