using JetBrains.Annotations;
using Verse;

// ReSharper disable InconsistentNaming

namespace BottledAbilities;

public sealed class BottledAbilitySettingEntry : IExposable {
    public string abilityDefName = string.Empty;
    public bool enabled = true;
    public AbilityCategory category = AbilityCategory.Utility;
    public int charges = 1;

    [UsedImplicitly]
    public BottledAbilitySettingEntry() { }

    public BottledAbilitySettingEntry(string abilityDefName, bool enabled, AbilityCategory category,
        int charges) {
        this.abilityDefName = abilityDefName;
        this.enabled = enabled;
        this.category = category;
        this.charges = charges;
    }

    public void ExposeData() {
        Scribe_Values.Look(ref abilityDefName, "abilityDefName", string.Empty);
        Scribe_Values.Look(ref enabled, "enabled", true);
        Scribe_Values.Look(ref category, "category", AbilityCategory.Utility);
        Scribe_Values.Look(ref charges, "charges", 1);
    }
}