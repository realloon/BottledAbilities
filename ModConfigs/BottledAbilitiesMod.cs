using JetBrains.Annotations;
using UnityEngine;
using RimWorld;
using Verse;

namespace BottledAbilities;

[UsedImplicitly]
public sealed class BottledAbilitiesMod : Mod {
    private static readonly List<Color> ColorPalette = BuildColorPalette();
    private const float Grid = 8f;
    private const float HalfGrid = 4f;
    private const float QuarterGrid = 2f;

    private const float TopPadding = Grid * 4f;
    private const float TabsAreaHeight = Grid * 5f;
    private const float ButtonHeight = Grid * 4f;
    private const float RowHeight = Grid * 4f;
    private const float BottomReservedHeight = Grid * 8f;

    private enum SettingsTab {
        AbilityJars,
        TemporaryAbilities,
        CategoryColors
    }

    private SettingsTab _activeTab = SettingsTab.AbilityJars;
    private string? _selectedAbilityPackageId;
    private Vector2 _abilityPackageListScrollPosition;
    private Vector2 _abilityListScrollPosition;

    public static BottledAbilitySettings Settings { get; private set; } = new();

    public BottledAbilitiesMod(ModContentPack content) : base(content) {
        Settings = GetSettings<BottledAbilitySettings>();
        Settings.InitializeIfNeeded();
    }

    public override string SettingsCategory() => "VortexBA_SettingsCategory".Translate();

    public override void DoSettingsWindowContents(Rect inRect) {
        var specs = BottledAbilityCatalog.GetAvailableSpecs();
        Settings.InitializeIfNeeded(specs);

        var y = TopPadding;
        DrawTabs(ref y, inRect.width);
        DrawPageHint(ref y, inRect.width);

        if (_activeTab == SettingsTab.AbilityJars) {
            DrawAbilityOptions(ref y, inRect.width, inRect.height - y - BottomReservedHeight, specs);
            DrawResetAbilityDefaultsButton(ref y, inRect.width, specs);
        } else if (_activeTab == SettingsTab.TemporaryAbilities) {
            DrawTemporaryAbilitySettings(ref y, inRect.width, inRect.height - y - BottomReservedHeight);
            DrawResetTemporaryDefaultsButton(ref y, inRect.width);
        } else if (_activeTab == SettingsTab.CategoryColors) {
            DrawCategoryColors(ref y, inRect.width);
            DrawResetColorDefaultsButton(ref y, inRect.width);
        }
    }

    private void DrawTabs(ref float y, float width) {
        var tabs = new[] {
            (Tab: SettingsTab.AbilityJars, Label: "VortexBA_SettingsTabAbilityList".Translate()),
            (Tab: SettingsTab.TemporaryAbilities, Label: "VortexBA_SettingsTabExpiry".Translate()),
            (Tab: SettingsTab.CategoryColors, Label: "VortexBA_SettingsTabCategory".Translate())
        };

        const float tabHeight = Grid * 4f;
        var tabWidth = (width - Grid * (tabs.Length - 1)) / tabs.Length;

        for (var i = 0; i < tabs.Length; i++) {
            var tab = tabs[i];
            var tabRect = new Rect(i * (tabWidth + Grid), y, tabWidth, tabHeight);
            var selected = _activeTab == tab.Tab;
            var background = selected
                ? new Color(1f, 1f, 1f, 0.12f)
                : new Color(1f, 1f, 1f, 0.035f);

            Widgets.DrawBoxSolid(tabRect, background);
            if (selected) {
                Widgets.DrawHighlightSelected(tabRect);
            } else {
                Widgets.DrawHighlightIfMouseover(tabRect);
            }

            if (Widgets.ButtonInvisible(tabRect)) {
                _activeTab = tab.Tab;
            }

            var oldAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(tabRect, tab.Label);
            Text.Anchor = oldAnchor;
        }

        y += TabsAreaHeight;
    }

    private static void DrawPageHint(ref float y, float width) {
        var hintRect = new Rect(0f, y, width, Grid * 4f);
        Widgets.Label(hintRect, "VortexBA_SettingsPageHint".Translate());
        y += Grid * 5f;
    }

    private void DrawTemporaryDurationControl(ref float y, float width) {
        const float buttonWidth = Grid * 20f;
        var rowRect = new Rect(0f, y, width, RowHeight);
        var durationTicks = Settings.GetTemporaryDurationTicks();
        var durationText = FormatDurationTicks(durationTicks);

        Widgets.Label(new Rect(rowRect.x, rowRect.y + HalfGrid, rowRect.width - buttonWidth - Grid, Grid * 3f),
            "VortexBA_SettingsTemporaryDurationLabel".Translate(durationText));

        var buttonRect = new Rect(rowRect.xMax - buttonWidth, rowRect.y, buttonWidth, ButtonHeight);
        if (Widgets.ButtonText(buttonRect, "VortexBA_SettingsTemporaryDurationButton".Translate())) {
            OpenTemporaryDurationSlider(durationTicks);
        }

        y += RowHeight + Grid;
    }

    private void DrawTemporaryAbilitySettings(ref float y, float width, float availableHeight) {
        var startY = y;
        var enabled = Settings.IsTemporaryDurationEnabled();
        var enabledRect = new Rect(0f, y, width, RowHeight);
        var wasEnabled = enabled;
        Widgets.CheckboxLabeled(enabledRect, "VortexBA_SettingsTemporaryExpiryEnabledLabel".Translate(), ref enabled);
        if (enabled != wasEnabled) {
            Settings.SetTemporaryDurationEnabled(enabled);
            WriteSettings();
        }

        y += RowHeight + Grid;
        if (enabled) {
            DrawTemporaryDurationControl(ref y, width);
        }

        var usedHeight = y - startY;
        y = startY + Mathf.Max(usedHeight, Mathf.Max(0f, availableHeight));
    }

    private void DrawResetAbilityDefaultsButton(ref float y, float width, IReadOnlyList<BottledAbilitySpec> specs) {
        y += Grid * 2f;

        if (Widgets.ButtonText(new Rect(0f, y, width, ButtonHeight), "VortexBA_SettingsResetAbilitiesButton".Translate())) {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "VortexBA_SettingsResetAbilitiesConfirm".Translate(),
                delegate {
                    Settings.ResetAbilityOptionsToDefault(specs);
                    WriteSettings();
                }));
        }

        y += ButtonHeight + HalfGrid;
    }

    private void DrawResetColorDefaultsButton(ref float y, float width) {
        y += Grid * 2f;

        if (Widgets.ButtonText(new Rect(0f, y, width, ButtonHeight), "VortexBA_SettingsResetColorsButton".Translate())) {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "VortexBA_SettingsResetColorsConfirm".Translate(),
                delegate {
                    Settings.ResetCategoryColorsToDefault();
                    WriteSettings();
                }));
        }

        y += ButtonHeight + HalfGrid;
    }

    private void DrawResetTemporaryDefaultsButton(ref float y, float width) {
        y += Grid * 2f;

        if (Widgets.ButtonText(new Rect(0f, y, width, ButtonHeight), "VortexBA_SettingsResetTemporaryButton".Translate())) {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "VortexBA_SettingsResetTemporaryConfirm".Translate(),
                delegate {
                    Settings.ResetTemporaryOptionsToDefault();
                    WriteSettings();
                }));
        }

        y += ButtonHeight + HalfGrid;
    }

    private void DrawCategoryColors(ref float y, float width) {
        const float labelWidth = 176f;
        const float colorPreviewSize = Grid * 3f;
        const float changeButtonWidth = Grid * 16f;

        foreach (var category in BottledAbilityCatalog.OrderedCategories) {
            var rowRect = new Rect(0f, y, width, RowHeight);
            var labelRect = new Rect(rowRect.x, rowRect.y + HalfGrid, labelWidth, Grid * 3f);
            Widgets.Label(labelRect, BottledAbilityCatalog.CategoryLabel(category));

            var colorRect = new Rect(labelRect.xMax + Grid, rowRect.y + HalfGrid, colorPreviewSize, colorPreviewSize);
            Widgets.DrawBoxSolid(colorRect, Settings.GetColor(category));

            var changeButtonRect = new Rect(colorRect.xMax + Grid, rowRect.y, changeButtonWidth, ButtonHeight);
            if (Widgets.ButtonText(changeButtonRect, "VortexBA_SettingsChangeButton".Translate())) {
                OpenColorPicker(category);
            }

            y += RowHeight + HalfGrid;
        }

        y += Grid;
    }

    private void DrawAbilityOptions(ref float y, float width, float availableHeight,
        IReadOnlyList<BottledAbilitySpec> specs) {
        var packageIds = GetOrderedPackageIds(specs);
        EnsureSelectedAbilityPackage(packageIds);

        var filteredSpecs = _selectedAbilityPackageId is null
            ? []
            : specs
                .Where(x => string.Equals(x.PackageId, _selectedAbilityPackageId, StringComparison.OrdinalIgnoreCase))
                .ToList();

        var leftColumnWidth = Mathf.Min(248f, Mathf.Max(200f, width * 0.30f));
        const float columnGap = Grid * 2f;
        var rightColumnX = leftColumnWidth + columnGap;
        var rightColumnWidth = Mathf.Max(240f, width - leftColumnWidth - columnGap);
        var columnsHeight = Mathf.Max(224f, availableHeight);

        Widgets.Label(new Rect(0f, y, leftColumnWidth, Grid * 3f), $"<b>{"VortexBA_SettingsSourceHeader".Translate()}</b>");
        Widgets.Label(new Rect(rightColumnX, y, rightColumnWidth, Grid * 3f), $"<b>{"VortexBA_SettingsAbilitiesHeader".Translate()}</b>");
        y += Grid * 3f + HalfGrid;

        DrawPackageSelectorColumn(0f, y, leftColumnWidth, columnsHeight, packageIds);
        DrawAbilityListColumn(rightColumnX, y, rightColumnWidth, columnsHeight, filteredSpecs);
        y += columnsHeight + Grid;
    }

    private void DrawPackageSelectorColumn(float x, float y, float width, float height,
        IReadOnlyList<string> packageIds) {
        var contentRect = new Rect(x, y, width, height);

        var rowCount = Mathf.Max(1, packageIds.Count);
        var viewHeight = HalfGrid + rowCount * (RowHeight + HalfGrid);
        var viewRect = new Rect(0f, 0f, contentRect.width - HalfGrid, Mathf.Max(contentRect.height, viewHeight));
        Widgets.BeginScrollView(contentRect, ref _abilityPackageListScrollPosition, viewRect, false);

        var rowY = HalfGrid;
        if (packageIds.Count == 0) {
            Widgets.Label(new Rect(Grid, rowY + HalfGrid, viewRect.width - Grid * 2f, Grid * 3f), "VortexBA_SettingsNoLoadedMods".Translate());
            Widgets.EndScrollView();
            return;
        }

        foreach (var packageId in packageIds) {
            var rowRect = new Rect(0f, rowY, viewRect.width - QuarterGrid, RowHeight);
            var isSelected = string.Equals(packageId, _selectedAbilityPackageId, StringComparison.OrdinalIgnoreCase);
            if (isSelected) {
                Widgets.DrawHighlightSelected(rowRect);
            } else {
                Widgets.DrawHighlightIfMouseover(rowRect);
            }

            if (Widgets.ButtonInvisible(rowRect)) {
                _selectedAbilityPackageId = packageId;
                _abilityListScrollPosition = Vector2.zero;
            }

            var labelRect = new Rect(rowRect.x + Grid, rowRect.y + HalfGrid, rowRect.width - Grid * 2f, Grid * 3f);
            Widgets.Label(labelRect, BottledAbilityCatalog.PackageLabel(packageId));
            rowY += RowHeight + HalfGrid;
        }

        Widgets.EndScrollView();
    }

    private void DrawAbilityListColumn(float x, float y, float width, float height,
        IReadOnlyList<BottledAbilitySpec> specs) {
        var contentRect = new Rect(x, y, width, height);

        var rows = Mathf.Max(1, specs.Count);
        var viewHeight = HalfGrid + rows * RowHeight;
        var viewRect = new Rect(0f, 0f, contentRect.width - Grid * 2f, Mathf.Max(contentRect.height, viewHeight));
        Widgets.BeginScrollView(contentRect, ref _abilityListScrollPosition, viewRect);

        if (specs.Count == 0) {
            Widgets.Label(new Rect(Grid, Grid, viewRect.width - Grid * 2f, Grid * 3f), "VortexBA_SettingsNoAbilitiesInPackage".Translate());
            Widgets.EndScrollView();
            return;
        }

        var rowY = QuarterGrid;
        foreach (var spec in specs) {
            DrawAbilityRow(spec, 0f, ref rowY, viewRect.width - QuarterGrid);
        }

        Widgets.EndScrollView();
    }

    private void EnsureSelectedAbilityPackage(IReadOnlyList<string> packageIds) {
        if (packageIds.Count == 0) {
            _selectedAbilityPackageId = null;
            return;
        }

        if (_selectedAbilityPackageId is null) {
            _selectedAbilityPackageId = packageIds[0];
            return;
        }

        foreach (var packageId in packageIds) {
            if (string.Equals(packageId, _selectedAbilityPackageId, StringComparison.OrdinalIgnoreCase)) {
                return;
            }
        }

        _selectedAbilityPackageId = packageIds[0];
    }

    private static List<string> GetOrderedPackageIds(IReadOnlyList<BottledAbilitySpec> specs) {
        return specs
            .Select(x => x.PackageId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(BottledAbilityCatalog.PackageLabel, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void DrawAbilityRow(BottledAbilitySpec spec, float x, ref float y, float width) {
        var rowRect = new Rect(x, y, width, RowHeight);
        var abilityDef = DefDatabase<AbilityDef>.GetNamedSilentFail(spec.AbilityDefName);
        var missing = abilityDef is null;

        var enabled = Settings.IsEnabled(spec.AbilityDefName);
        var categoryWidth = Grid * 18f;
        var chargeButtonWidth = Grid * 7f;
        var reservedWidth = categoryWidth + Grid + chargeButtonWidth;

        if (reservedWidth > width * 0.55f) {
            categoryWidth = Grid * 14f;
            chargeButtonWidth = Grid * 6f;
            reservedWidth = categoryWidth + Grid + chargeButtonWidth;
        }

        var controlsStartX = rowRect.x + Mathf.Max(120f, width - reservedWidth);
        var checkboxRect = new Rect(rowRect.x, rowRect.y, Mathf.Max(120f, controlsStartX - rowRect.x - Grid), RowHeight);

        var label = abilityDef?.label ?? GenText.SplitCamelCase(spec.AbilityDefName);
        if (missing) {
            label = "VortexBA_SettingsAbilityMissing".Translate(label);
        }

        var wasEnabled = enabled;
        Widgets.CheckboxLabeled(checkboxRect, label, ref enabled);
        if (enabled != wasEnabled) {
            Settings.SetEnabled(spec.AbilityDefName, enabled);
        }

        var category = Settings.GetCategory(spec.AbilityDefName);
        var categoryButtonRect = new Rect(controlsStartX, rowRect.y, categoryWidth, RowHeight);
        if (Widgets.ButtonText(categoryButtonRect, BottledAbilityCatalog.CategoryLabel(category))) {
            OpenCategoryMenu(spec.AbilityDefName);
        }

        var charges = Settings.GetCharges(spec.AbilityDefName);
        var chargeButtonRect = new Rect(categoryButtonRect.xMax + Grid, rowRect.y, chargeButtonWidth, RowHeight);
        if (Widgets.ButtonText(chargeButtonRect, $"x{charges}")) {
            OpenChargesSlider(spec.AbilityDefName, charges);
        }

        y += RowHeight;
    }

    private void OpenCategoryMenu(string abilityDefName) {
        var options = BottledAbilityCatalog
            .OrderedCategories
            .Select(captured => new FloatMenuOption(
                BottledAbilityCatalog.CategoryLabel(captured), delegate {
                    Settings.SetCategory(abilityDefName, captured);
                    WriteSettings();
                }))
            .ToList();

        Find.WindowStack.Add(new FloatMenu(options));
    }

    private void OpenChargesSlider(string abilityDefName, int currentCharges) {
        Find.WindowStack.Add(new Dialog_Slider(
            val => "VortexBA_SettingsChargesSliderLabel".Translate(val),
            BottledAbilitySettings.MinCharges,
            BottledAbilitySettings.MaxCharges,
            delegate(int selected) {
                Settings.SetCharges(abilityDefName, selected);
                WriteSettings();
            },
            currentCharges));
    }

    private void OpenTemporaryDurationSlider(int currentDurationTicks) {
        Find.WindowStack.Add(new Dialog_Slider(
            val => "VortexBA_SettingsTemporaryDurationSliderLabel".Translate(FormatDurationTicks(val)),
            BottledAbilitySettings.MinTemporaryDurationTicks,
            BottledAbilitySettings.MaxTemporaryDurationTicks,
            delegate(int selected) {
                Settings.SetTemporaryDurationTicks(selected);
                WriteSettings();
            },
            currentDurationTicks));
    }

    private void OpenColorPicker(AbilityCategory category) {
        Find.WindowStack.Add(new Dialog_ChooseColor(
            "VortexBA_SettingsColorPickerTitle".Translate(BottledAbilityCatalog.CategoryLabel(category)),
            Settings.GetColor(category),
            ColorPalette,
            delegate(Color chosen) {
                Settings.SetColor(category, chosen);
                WriteSettings();
            }));
    }

    private static List<Color> BuildColorPalette() {
        var colors = new List<Color> {
            Color.white,
            Color.gray,
            Color.black,
            Color.red,
            Color.green,
            Color.blue,
            Color.cyan,
            Color.magenta,
            Color.yellow
        };

        for (var h = 0f; h < 1f; h += 1f / 24f) {
            foreach (var saturation in new[] { 0.35f, 0.55f, 0.75f, 0.95f }) {
                colors.AddRange(new[] { 0.55f, 0.75f, 0.95f }
                    .Select(value => Color.HSVToRGB(h, saturation, value)));
            }
        }

        return colors;
    }

    private static string FormatDurationTicks(int ticks) {
        return ticks.ToStringTicksToPeriod(allowSeconds: true, shortForm: true, canUseDecimals: true,
            allowYears: true, canUseDecimalsShortForm: true);
    }
}
