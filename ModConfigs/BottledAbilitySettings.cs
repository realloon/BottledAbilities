using UnityEngine;
using Verse;

namespace BottledAbilities;

public sealed class BottledAbilitySettings : ModSettings {
    public const int MinCharges = 1;
    public const int MaxCharges = 9;

    private List<BottledAbilitySettingEntry> _abilityEntries = [];
    private List<BottledAbilityCategoryColorEntry> _categoryColorEntries = [];

    [Unsaved]
    private Dictionary<string, BottledAbilitySettingEntry>? _abilityByName;

    [Unsaved]
    private Dictionary<AbilityCategory, BottledAbilityCategoryColorEntry>? _colorByCategory;

    public void InitializeIfNeeded(IReadOnlyList<BottledAbilitySpec>? specs = null) {
        _abilityEntries.RemoveAll(x => x == null || x.abilityDefName.NullOrEmpty());
        _categoryColorEntries.RemoveAll(x => x == null);

        RebuildCaches();
        EnsureColorEntries();

        if (specs is null) {
            return;
        }

        SyncAbilityEntries(specs);
    }

    public void ResetAbilityOptionsToDefault(IReadOnlyList<BottledAbilitySpec>? specs = null) {
        specs ??= BottledAbilityCatalog.GetAvailableSpecs();
        _abilityEntries = [];

        foreach (var spec in specs) {
            _abilityEntries.Add(new BottledAbilitySettingEntry(
                spec.AbilityDefName,
                BottledAbilityCatalog.IsDefaultEnabled,
                spec.DefaultCategory,
                Mathf.Clamp(spec.DefaultCharges, MinCharges, MaxCharges)));
        }

        RebuildCaches();
    }

    public void ResetCategoryColorsToDefault() {
        _categoryColorEntries = [];

        foreach (var category in BottledAbilityCatalog.OrderedCategories) {
            _categoryColorEntries.Add(new BottledAbilityCategoryColorEntry(
                category,
                BottledAbilityCatalog.DefaultColor(category)));
        }

        RebuildCaches();
    }

    public bool IsEnabled(string abilityDefName) {
        EnsureCaches();
        return _abilityByName!.GetValueOrDefault(abilityDefName)?.enabled ?? true;
    }

    public void SetEnabled(string abilityDefName, bool enabled) {
        var entry = GetOrCreateAbilityEntry(abilityDefName);

        entry.enabled = enabled;
    }

    public AbilityCategory GetCategory(string abilityDefName) {
        EnsureCaches();

        if (_abilityByName!.TryGetValue(abilityDefName, out var entry)) {
            return entry.category;
        }

        return BottledAbilityCatalog.FindSpec(abilityDefName)?.DefaultCategory ?? AbilityCategory.Utility;
    }

    public void SetCategory(string abilityDefName, AbilityCategory category) {
        var entry = GetOrCreateAbilityEntry(abilityDefName);

        entry.category = category;
    }

    public int GetCharges(string abilityDefName) {
        EnsureCaches();
        var value = _abilityByName!.GetValueOrDefault(abilityDefName)?.charges ?? 1;
        return Mathf.Clamp(value, MinCharges, MaxCharges);
    }

    public void SetCharges(string abilityDefName, int charges) {
        var entry = GetOrCreateAbilityEntry(abilityDefName);
        var clamped = Mathf.Clamp(charges, MinCharges, MaxCharges);

        entry.charges = clamped;
    }

    public Color GetColor(AbilityCategory category) {
        EnsureCaches();

        return _colorByCategory!.TryGetValue(category, out var entry)
            ? entry.color
            : BottledAbilityCatalog.DefaultColor(category);
    }

    public void SetColor(AbilityCategory category, Color color) {
        EnsureCaches();

        if (!_colorByCategory!.TryGetValue(category, out var entry)) {
            entry = new BottledAbilityCategoryColorEntry(category, color);
            _categoryColorEntries.Add(entry);
            _colorByCategory[category] = entry;
            return;
        }

        entry.color = color;
    }

    private BottledAbilitySettingEntry GetOrCreateAbilityEntry(string abilityDefName) {
        EnsureCaches();

        if (_abilityByName!.TryGetValue(abilityDefName, out var entry)) {
            return entry;
        }

        var category = BottledAbilityCatalog.FindSpec(abilityDefName)?.DefaultCategory ??
                       AbilityCategory.Utility;
        var defaultCharges = BottledAbilityCatalog.FindSpec(abilityDefName)?.DefaultCharges ?? 1;
        entry = new BottledAbilitySettingEntry(abilityDefName, true, category,
            Mathf.Clamp(defaultCharges, MinCharges, MaxCharges));
        _abilityEntries.Add(entry);
        _abilityByName[abilityDefName] = entry;

        return entry;
    }

    private void EnsureCaches() {
        if (_abilityByName is null || _colorByCategory is null) {
            RebuildCaches();
        }
    }

    private void RebuildCaches() {
        _abilityByName = new Dictionary<string, BottledAbilitySettingEntry>(StringComparer.Ordinal);
        _colorByCategory = new Dictionary<AbilityCategory, BottledAbilityCategoryColorEntry>();

        foreach (var entry in _abilityEntries) {
            entry.charges = Mathf.Clamp(entry.charges, MinCharges, MaxCharges);
            _abilityByName[entry.abilityDefName] = entry;
        }

        foreach (var entry in _categoryColorEntries) {
            _colorByCategory[entry.category] = entry;
        }
    }

    private void EnsureColorEntries() {
        var changed = false;
        foreach (var category in BottledAbilityCatalog.OrderedCategories) {
            if (_colorByCategory!.ContainsKey(category)) continue;

            _categoryColorEntries.Add(new BottledAbilityCategoryColorEntry(
                category,
                BottledAbilityCatalog.DefaultColor(category)));
            changed = true;
        }

        if (changed) {
            RebuildCaches();
        }
    }

    private void SyncAbilityEntries(IReadOnlyList<BottledAbilitySpec> specs) {
        if (specs.Count == 0) return;

        var changed = false;

        if (_abilityEntries.Count == 0) {
            foreach (var spec in specs) {
                _abilityEntries.Add(new BottledAbilitySettingEntry(
                    spec.AbilityDefName,
                    BottledAbilityCatalog.IsDefaultEnabled,
                    spec.DefaultCategory,
                    Mathf.Clamp(spec.DefaultCharges, MinCharges, MaxCharges)));
            }

            changed = true;
        } else {
            foreach (var spec in specs) {
                if (_abilityByName!.ContainsKey(spec.AbilityDefName)) continue;

                _abilityEntries.Add(new BottledAbilitySettingEntry(
                    spec.AbilityDefName,
                    BottledAbilityCatalog.IsDefaultEnabled,
                    spec.DefaultCategory,
                    Mathf.Clamp(spec.DefaultCharges, MinCharges, MaxCharges)));
                changed = true;
            }
        }

        if (changed) {
            RebuildCaches();
        }
    }

    public override void ExposeData() {
        Scribe_Collections.Look(ref _abilityEntries, "abilityEntries", LookMode.Deep);
        Scribe_Collections.Look(ref _categoryColorEntries, "categoryColorEntries", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit) {
            InitializeIfNeeded();
        }
    }
}