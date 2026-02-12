using JetBrains.Annotations;
using UnityEngine;
using Verse;

// ReSharper disable InconsistentNaming

namespace BottledAbilities;

public sealed class BottledAbilitySettingEntry : IExposable {
    public string abilityDefName = string.Empty;
    public bool enabled = true;
    public BottledAbilityCategory category = BottledAbilityCategory.Utility;
    public int charges = 1;

    [UsedImplicitly]
    public BottledAbilitySettingEntry() { }

    public BottledAbilitySettingEntry(string abilityDefName, bool enabled, BottledAbilityCategory category,
        int charges) {
        this.abilityDefName = abilityDefName;
        this.enabled = enabled;
        this.category = category;
        this.charges = charges;
    }

    public void ExposeData() {
        Scribe_Values.Look(ref abilityDefName, "abilityDefName", string.Empty);
        abilityDefName ??= string.Empty;
        Scribe_Values.Look(ref enabled, "enabled", true);
        Scribe_Values.Look(ref category, "category", BottledAbilityCategory.Utility);
        Scribe_Values.Look(ref charges, "charges", 1);
        charges = Mathf.Clamp(charges, BottledAbilitySettings.MinCharges, BottledAbilitySettings.MaxCharges);
    }
}