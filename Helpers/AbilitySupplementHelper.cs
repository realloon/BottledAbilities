using RimWorld;
using Verse;

namespace BottledAbilities.Helpers;

internal static class AbilitySupplementHelper {
    public static bool IsOwnedAbilityCooldownRefreshEnabled() {
        var settings = BottledAbilitiesMod.Settings;
        settings.InitializeIfNeeded();
        return settings.OwnedAbilityCooldownRefreshEnabled;
    }

    public static bool HasOwnedPermanentAbility(Pawn pawn, AbilityDef abilityDef) {
        return GetOwnedPermanentAbility(pawn, abilityDef) is not null;
    }

    public static bool ShouldAllowCooldownSupplementIngestion(Pawn pawn, AbilityDef abilityDef) {
        if (!IsOwnedAbilityCooldownRefreshEnabled()) {
            return false;
        }

        var ability = GetOwnedPermanentAbility(pawn, abilityDef);
        return ability is not null && ability.OnCooldown;
    }

    public static bool TryRefreshCooldownFromSupplement(Pawn pawn, AbilityDef abilityDef) {
        if (!ShouldAllowCooldownSupplementIngestion(pawn, abilityDef)) {
            return false;
        }

        var ability = GetOwnedPermanentAbility(pawn, abilityDef);
        ability?.ResetCooldown();
        return true;
    }

    private static Ability? GetOwnedPermanentAbility(Pawn pawn, AbilityDef abilityDef) {
        return pawn.abilities?.GetAbility(abilityDef);
    }
}