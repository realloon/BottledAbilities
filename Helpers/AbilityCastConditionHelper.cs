using RimWorld;

namespace BottledAbilities.Helpers;

internal static class AbilityCastConditionHelper {
    public static bool ShouldIgnoreCastConditions(Ability ability) {
        var settings = BottledAbilitiesMod.Settings;
        settings.InitializeIfNeeded();

        if (!settings.IgnoreCastConditionsForBottledAbilities) {
            return false;
        }

        var pawn = ability.pawn;
        return pawn?.health?.hediffSet?.hediffs?
            .OfType<Hediff_BottledAbility>()
            .Any(h => h.AbilityDef == ability.def) == true;
    }
}