using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace BottledAbilities;

public sealed class BottledAbilitySettingEntry : IExposable {
    public string abilityDefName = string.Empty;
    public bool enabled = true;
    public BottledAbilityCategory category = BottledAbilityCategory.Utility;
    public int charges = 1;

    [UsedImplicitly]
    public BottledAbilitySettingEntry() {
    }

    public BottledAbilitySettingEntry(string abilityDefName, bool enabled, BottledAbilityCategory category, int charges) {
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
        if (charges < 1) charges = 1;
    }
}

public sealed class BottledAbilityCategoryColorEntry : IExposable {
    public BottledAbilityCategory category;
    public Color color = Color.white;

    [UsedImplicitly]
    public BottledAbilityCategoryColorEntry() {
    }

    public BottledAbilityCategoryColorEntry(BottledAbilityCategory category, Color color) {
        this.category = category;
        this.color = color;
    }

    public void ExposeData() {
        Scribe_Values.Look(ref category, "category", BottledAbilityCategory.Utility);
        Scribe_Values.Look(ref color, "color", Color.white);
    }
}

public sealed class BottledAbilitySettings : ModSettings {
    private const string CustomPresetLabel = "Custom";

    public string activePreset = BottledAbilityPresetKind.Balanced.ToString();

    private List<BottledAbilitySettingEntry> abilityEntries = [];
    private List<BottledAbilityCategoryColorEntry> categoryColorEntries = [];

    [Unsaved] private Dictionary<string, BottledAbilitySettingEntry>? abilityByName;
    [Unsaved] private Dictionary<BottledAbilityCategory, BottledAbilityCategoryColorEntry>? colorByCategory;

    public override void ExposeData() {
        Scribe_Values.Look(ref activePreset, "activePreset", BottledAbilityPresetKind.Balanced.ToString());
        Scribe_Collections.Look(ref abilityEntries, "abilityEntries", LookMode.Deep);
        Scribe_Collections.Look(ref categoryColorEntries, "categoryColorEntries", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit) {
            InitializeIfNeeded();
        }
    }

    public void InitializeIfNeeded(IReadOnlyList<BottledAbilitySpec>? specs = null) {
        abilityEntries ??= [];
        categoryColorEntries ??= [];

        abilityEntries.RemoveAll(x => x == null || x.abilityDefName.NullOrEmpty());
        categoryColorEntries.RemoveAll(x => x == null);

        RebuildCaches();
        EnsureColorEntries();

        if (specs is null) {
            return;
        }

        SyncAbilityEntries(specs);
    }

    public void ApplyPreset(BottledAbilityPresetKind preset, IReadOnlyList<BottledAbilitySpec>? specs = null) {
        abilityEntries = [];
        categoryColorEntries = [];
        specs ??= BottledAbilityCatalog.GetAvailableSpecs();

        foreach (var spec in specs) {
            abilityEntries.Add(new BottledAbilitySettingEntry(
                spec.AbilityDefName,
                BottledAbilityCatalog.DefaultEnabled(preset, spec.DefaultCategory),
                spec.DefaultCategory,
                spec.DefaultCharges));
        }

        foreach (var category in BottledAbilityCatalog.OrderedCategories) {
            categoryColorEntries.Add(new BottledAbilityCategoryColorEntry(
                category,
                BottledAbilityCatalog.DefaultColor(preset, category)));
        }

        activePreset = preset.ToString();
        RebuildCaches();
    }

    public bool IsEnabled(string abilityDefName) {
        EnsureCaches();
        return abilityByName!.GetValueOrDefault(abilityDefName)?.enabled ?? true;
    }

    public void SetEnabled(string abilityDefName, bool enabled) {
        var entry = GetOrCreateAbilityEntry(abilityDefName);
        if (entry.enabled == enabled) return;

        entry.enabled = enabled;
        MarkCustom();
    }

    public BottledAbilityCategory GetCategory(string abilityDefName) {
        EnsureCaches();

        if (abilityByName!.TryGetValue(abilityDefName, out var entry)) {
            return entry.category;
        }

        return BottledAbilityCatalog.FindSpec(abilityDefName)?.DefaultCategory ?? BottledAbilityCategory.Utility;
    }

    public void SetCategory(string abilityDefName, BottledAbilityCategory category) {
        var entry = GetOrCreateAbilityEntry(abilityDefName);
        if (entry.category == category) return;

        entry.category = category;
        MarkCustom();
    }

    public int GetCharges(string abilityDefName) {
        EnsureCaches();
        var value = abilityByName!.GetValueOrDefault(abilityDefName)?.charges ?? 1;
        return Mathf.Max(1, value);
    }

    public void SetCharges(string abilityDefName, int charges) {
        var entry = GetOrCreateAbilityEntry(abilityDefName);
        var clamped = Mathf.Clamp(charges, 1, 999);
        if (entry.charges == clamped) return;

        entry.charges = clamped;
        MarkCustom();
    }

    public Color GetColor(BottledAbilityCategory category) {
        EnsureCaches();

        if (colorByCategory!.TryGetValue(category, out var entry)) {
            return entry.color;
        }

        return BottledAbilityCatalog.DefaultColor(BottledAbilityPresetKind.Balanced, category);
    }

    public void SetColor(BottledAbilityCategory category, Color color) {
        EnsureCaches();

        if (!colorByCategory!.TryGetValue(category, out var entry)) {
            entry = new BottledAbilityCategoryColorEntry(category, color);
            categoryColorEntries.Add(entry);
            colorByCategory[category] = entry;
            MarkCustom();
            return;
        }

        if (entry.color == color) return;

        entry.color = color;
        MarkCustom();
    }

    public bool IsCustomPreset => string.Equals(activePreset, CustomPresetLabel, StringComparison.OrdinalIgnoreCase);

    public void MarkCustom() {
        activePreset = CustomPresetLabel;
    }

    private BottledAbilitySettingEntry GetOrCreateAbilityEntry(string abilityDefName) {
        EnsureCaches();

        if (abilityByName!.TryGetValue(abilityDefName, out var entry)) {
            return entry;
        }

        var category = BottledAbilityCatalog.FindSpec(abilityDefName)?.DefaultCategory ?? BottledAbilityCategory.Utility;
        var defaultCharges = BottledAbilityCatalog.FindSpec(abilityDefName)?.DefaultCharges ?? 1;
        entry = new BottledAbilitySettingEntry(abilityDefName, true, category, defaultCharges);
        abilityEntries.Add(entry);
        abilityByName[abilityDefName] = entry;

        return entry;
    }

    private void EnsureCaches() {
        if (abilityByName is null || colorByCategory is null) {
            RebuildCaches();
        }
    }

    private void RebuildCaches() {
        abilityByName = new Dictionary<string, BottledAbilitySettingEntry>(StringComparer.Ordinal);
        colorByCategory = new Dictionary<BottledAbilityCategory, BottledAbilityCategoryColorEntry>();

        foreach (var entry in abilityEntries) {
            abilityByName[entry.abilityDefName] = entry;
        }

        foreach (var entry in categoryColorEntries) {
            colorByCategory[entry.category] = entry;
        }
    }

    private void EnsureColorEntries() {
        var presetForColor = IsCustomPreset
            ? BottledAbilityPresetKind.Balanced
            : TryGetPresetOrDefault(activePreset, BottledAbilityPresetKind.Balanced);

        var changed = false;
        foreach (var category in BottledAbilityCatalog.OrderedCategories) {
            if (colorByCategory!.ContainsKey(category)) continue;

            categoryColorEntries.Add(new BottledAbilityCategoryColorEntry(
                category,
                BottledAbilityCatalog.DefaultColor(presetForColor, category)));
            changed = true;
        }

        if (changed) {
            RebuildCaches();
        }
    }

    private void SyncAbilityEntries(IReadOnlyList<BottledAbilitySpec> specs) {
        if (specs.Count == 0) return;

        var presetForDefaults = IsCustomPreset
            ? BottledAbilityPresetKind.Balanced
            : TryGetPresetOrDefault(activePreset, BottledAbilityPresetKind.Balanced);

        var changed = false;

        if (abilityEntries.Count == 0) {
            foreach (var spec in specs) {
                abilityEntries.Add(new BottledAbilitySettingEntry(
                    spec.AbilityDefName,
                    BottledAbilityCatalog.DefaultEnabled(presetForDefaults, spec.DefaultCategory),
                    spec.DefaultCategory,
                    spec.DefaultCharges));
            }

            changed = true;
        }
        else {
            foreach (var spec in specs) {
                if (abilityByName!.ContainsKey(spec.AbilityDefName)) continue;

                abilityEntries.Add(new BottledAbilitySettingEntry(
                    spec.AbilityDefName,
                    BottledAbilityCatalog.DefaultEnabled(presetForDefaults, spec.DefaultCategory),
                    spec.DefaultCategory,
                    spec.DefaultCharges));
                changed = true;
            }
        }

        if (changed) {
            RebuildCaches();
        }
    }

    private static BottledAbilityPresetKind TryGetPresetOrDefault(string presetText, BottledAbilityPresetKind fallback) {
        if (Enum.TryParse(presetText, true, out BottledAbilityPresetKind parsed)) {
            return parsed;
        }

        return fallback;
    }
}
