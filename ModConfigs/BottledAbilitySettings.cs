using UnityEngine;
using Verse;

namespace BottledAbilities;

public sealed class BottledAbilitySettings : ModSettings {
    public const int MinCharges = 1;
    public const int MaxCharges = 9;
    private const int DefaultTemporaryDurationTicks = 60000;
    public const int MinTemporaryDurationTicks = 2500;
    public const int MaxTemporaryDurationTicks = 900000;

    private List<BottledAbilitySettingEntry> _abilityEntries = [];
    private List<BottledAbilityCategoryColorEntry> _categoryColorEntries = [];
    private bool _temporaryDurationEnabled = true;
    private int _temporaryDurationTicks = DefaultTemporaryDurationTicks;

    [Unsaved]
    private Dictionary<string, BottledAbilitySettingEntry>? _abilityByName;

    [Unsaved]
    private Dictionary<AbilityCategory, BottledAbilityCategoryColorEntry>? _colorByCategory;

    public void InitializeIfNeeded(IReadOnlyList<BottledAbilitySpec>? specs = null) {
        _temporaryDurationTicks = ClampTemporaryDurationTicks(_temporaryDurationTicks);

        if (_abilityByName is null || _colorByCategory is null) {
            _abilityEntries.RemoveAll(x => x == null || x.abilityDefName.NullOrEmpty());
            _categoryColorEntries.RemoveAll(x => x == null);

            RebuildCaches();
            EnsureColorEntries();
        }

        if (specs is null) {
            return;
        }

        SyncAbilityEntries(specs);
    }

    public void ResetAbilityOptionsToDefault(IReadOnlyList<BottledAbilitySpec>? specs = null) {
        specs ??= BottledAbilityCatalog.GetAvailableSpecs();
        _abilityEntries = [];

        foreach (var spec in specs) {
            _abilityEntries.Add(CreateEntryFromSpec(spec));
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

    public bool IsTemporaryDurationEnabled {
        get => _temporaryDurationEnabled;
        set => _temporaryDurationEnabled = value;
    }

    public int TemporaryDurationTicks {
        get => ClampTemporaryDurationTicks(_temporaryDurationTicks);
        set => _temporaryDurationTicks = ClampTemporaryDurationTicks(value);
    }

    public void ResetTemporaryOptionsToDefault() {
        (_temporaryDurationEnabled, _temporaryDurationTicks) = (true, DefaultTemporaryDurationTicks);
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
        return ClampCharges(value);
    }

    public void SetCharges(string abilityDefName, int charges) {
        var entry = GetOrCreateAbilityEntry(abilityDefName);
        var clamped = ClampCharges(charges);

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

        var spec = BottledAbilityCatalog.FindSpec(abilityDefName);
        var category = spec?.DefaultCategory ?? AbilityCategory.Utility;
        var defaultCharges = spec?.DefaultCharges ?? 1;
        entry = new BottledAbilitySettingEntry(abilityDefName, true, category,
            ClampCharges(defaultCharges));
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
            entry.charges = ClampCharges(entry.charges);
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
        var shouldSeedAll = _abilityEntries.Count == 0;

        foreach (var spec in specs) {
            if (!shouldSeedAll && _abilityByName!.ContainsKey(spec.AbilityDefName)) continue;

            _abilityEntries.Add(CreateEntryFromSpec(spec));
            changed = true;
        }

        if (changed) {
            RebuildCaches();
        }
    }

    public override void ExposeData() {
        Scribe_Collections.Look(ref _abilityEntries, "abilityEntries", LookMode.Deep);
        Scribe_Collections.Look(ref _categoryColorEntries, "categoryColorEntries", LookMode.Deep);
        Scribe_Values.Look(ref _temporaryDurationEnabled, "temporaryDurationEnabled", true);
        Scribe_Values.Look(ref _temporaryDurationTicks, "temporaryDurationTicks", DefaultTemporaryDurationTicks);

        if (Scribe.mode == LoadSaveMode.PostLoadInit) {
            InitializeIfNeeded();
        }
    }

    private static int ClampCharges(int charges) {
        return Mathf.Clamp(charges, MinCharges, MaxCharges);
    }

    private static int ClampTemporaryDurationTicks(int durationTicks) {
        return Mathf.Clamp(durationTicks, MinTemporaryDurationTicks, MaxTemporaryDurationTicks);
    }

    private static BottledAbilitySettingEntry CreateEntryFromSpec(BottledAbilitySpec spec) {
        return new BottledAbilitySettingEntry(
            spec.AbilityDefName,
            BottledAbilityCatalog.IsDefaultEnabled,
            spec.DefaultCategory,
            ClampCharges(spec.DefaultCharges));
    }
}
