using JetBrains.Annotations;
using UnityEngine;
using RimWorld;
using Verse;

namespace BottledAbilities;

[UsedImplicitly]
public sealed class BottledAbilitiesMod : Mod {
    private static readonly List<Color> ColorPalette = BuildColorPalette();
    private const float TopPadding = 32f;
    private const float TabsAreaHeight = 40f;

    private enum SettingsTab {
        AbilityJars,
        CategoryColors
    }

    private SettingsTab _activeTab = SettingsTab.AbilityJars;
    private string? _selectedAbilityPackageId;
    private Vector2 _abilityPackageListScrollPosition;
    private Vector2 _abilityListScrollPosition;

    public static BottledAbilitySettings Settings { get; private set; } = new();

    public BottledAbilitiesMod(ModContentPack content) : base(content) {
        BottledAbilities.EnsurePatched();
        Settings = GetSettings<BottledAbilitySettings>();
        Settings.InitializeIfNeeded();
    }

    public override string SettingsCategory() => "Bottled Abilities";

    public override void DoSettingsWindowContents(Rect inRect) {
        var specs = BottledAbilityCatalog.GetAvailableSpecs();
        Settings.InitializeIfNeeded(specs);

        var y = TopPadding;
        DrawTabs(ref y, inRect.width);
        DrawPageHint(ref y, inRect.width);

        if (_activeTab == SettingsTab.AbilityJars) {
            DrawAbilityOptions(ref y, inRect.width, inRect.height - y - 76f, specs);
            DrawResetAbilityDefaultsButton(ref y, inRect.width, specs);
        } else if (_activeTab == SettingsTab.CategoryColors) {
            DrawCategoryColors(ref y, inRect.width);
            DrawResetColorDefaultsButton(ref y, inRect.width);
        }
    }

    private void DrawTabs(ref float y, float width) {
        var tabs = new[] {
            (Tab: SettingsTab.AbilityJars, Label: "Ability Jars"),
            (Tab: SettingsTab.CategoryColors, Label: "Category Colors")
        };

        const float tabGap = 10f;
        const float tabHeight = 30f;
        var tabWidth = (width - tabGap * (tabs.Length - 1)) / tabs.Length;

        for (var i = 0; i < tabs.Length; i++) {
            var tab = tabs[i];
            var tabRect = new Rect(i * (tabWidth + tabGap), y, tabWidth, tabHeight);
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
        var hintRect = new Rect(0f, y, width, 32f);
        Widgets.Label(hintRect, "Changes apply after restarting the game. Disabled jars are removed on startup.");
        y += 36f;
    }

    private void DrawResetAbilityDefaultsButton(ref float y, float width, IReadOnlyList<BottledAbilitySpec> specs) {
        y += 18f;

        if (Widgets.ButtonText(new Rect(0f, y, width, 28f), "Reset Ability Jars To Default")) {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "Reset all ability jar options to default values?",
                delegate {
                    Settings.ResetAbilityOptionsToDefault(specs);
                    WriteSettings();
                }));
        }

        y += 34f;
    }

    private void DrawResetColorDefaultsButton(ref float y, float width) {
        y += 18f;

        if (Widgets.ButtonText(new Rect(0f, y, width, 28f), "Reset Category Colors To Default")) {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "Reset all category colors to default values?",
                delegate {
                    Settings.ResetCategoryColorsToDefault();
                    WriteSettings();
                }));
        }

        y += 34f;
    }

    private void DrawCategoryColors(ref float y, float width) {
        foreach (var category in BottledAbilityCatalog.OrderedCategories) {
            var rowRect = new Rect(0f, y, width, 28f);
            var labelRect = new Rect(rowRect.x, rowRect.y + 5f, 180f, 22f);
            Widgets.Label(labelRect, BottledAbilityCatalog.CategoryLabel(category));

            var colorRect = new Rect(rowRect.x + 190f, rowRect.y + 4f, 22f, 22f);
            Widgets.DrawBoxSolid(colorRect, Settings.GetColor(category));

            if (Widgets.ButtonText(new Rect(rowRect.x + 220f, rowRect.y, 130f, 28f), "Change")) {
                OpenColorPicker(category);
            }

            y += 30f;
        }

        y += 6f;
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

        var leftColumnWidth = Mathf.Min(250f, Mathf.Max(200f, width * 0.30f));
        const float columnGap = 14f;
        var rightColumnX = leftColumnWidth + columnGap;
        var rightColumnWidth = Mathf.Max(240f, width - leftColumnWidth - columnGap);
        var columnsHeight = Mathf.Max(220f, availableHeight);

        Widgets.Label(new Rect(0f, y, leftColumnWidth, 24f), "<b>Source</b>");
        Widgets.Label(new Rect(rightColumnX, y, rightColumnWidth, 24f), "<b>Abilities</b>");
        y += 28f;

        DrawPackageSelectorColumn(0f, y, leftColumnWidth, columnsHeight, packageIds);
        DrawAbilityListColumn(rightColumnX, y, rightColumnWidth, columnsHeight, filteredSpecs);
        y += columnsHeight + 8f;
    }

    private void DrawPackageSelectorColumn(float x, float y, float width, float height,
        IReadOnlyList<string> packageIds) {
        var rowHeight = 28f;
        var rowGap = 2f;
        var contentRect = new Rect(x, y, width, height);

        var rowCount = Mathf.Max(1, packageIds.Count);
        var viewHeight = 4f + rowCount * (rowHeight + rowGap);
        var viewRect = new Rect(0f, 0f, contentRect.width - 4f, Mathf.Max(contentRect.height, viewHeight));
        Widgets.BeginScrollView(contentRect, ref _abilityPackageListScrollPosition, viewRect, false);

        var rowY = 2f;
        if (packageIds.Count == 0) {
            Widgets.Label(new Rect(6f, rowY + 4f, viewRect.width - 12f, 22f), "No loaded mods.");
            Widgets.EndScrollView();
            return;
        }

        foreach (var packageId in packageIds) {
            var rowRect = new Rect(0f, rowY, viewRect.width - 2f, rowHeight);
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

            var labelRect = new Rect(rowRect.x + 6f, rowRect.y + 5f, rowRect.width - 12f, 22f);
            Widgets.Label(labelRect, BottledAbilityCatalog.PackageLabel(packageId));
            rowY += rowHeight + rowGap;
        }

        Widgets.EndScrollView();
    }

    private void DrawAbilityListColumn(float x, float y, float width, float height,
        IReadOnlyList<BottledAbilitySpec> specs) {
        var contentRect = new Rect(x, y, width, height);

        var rows = Mathf.Max(1, specs.Count);
        var viewHeight = 4f + rows * 30f;
        var viewRect = new Rect(0f, 0f, contentRect.width - 16f, Mathf.Max(contentRect.height, viewHeight));
        Widgets.BeginScrollView(contentRect, ref _abilityListScrollPosition, viewRect);

        if (specs.Count == 0) {
            Widgets.Label(new Rect(6f, 8f, viewRect.width - 12f, 22f), "No abilities in this package.");
            Widgets.EndScrollView();
            return;
        }

        var rowY = 2f;
        foreach (var spec in specs) {
            DrawAbilityRow(spec, 0f, ref rowY, viewRect.width - 2f);
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
        var rowRect = new Rect(x, y, width, 28f);
        var abilityDef = DefDatabase<AbilityDef>.GetNamedSilentFail(spec.AbilityDefName);
        var missing = abilityDef is null;

        var enabled = Settings.IsEnabled(spec.AbilityDefName);
        var categoryWidth = 140f;
        var chargeButtonWidth = 58f;
        const float controlsGap = 10f;
        var reservedWidth = categoryWidth + controlsGap + chargeButtonWidth;

        if (reservedWidth > width * 0.55f) {
            categoryWidth = 116f;
            chargeButtonWidth = 52f;
            reservedWidth = categoryWidth + controlsGap + chargeButtonWidth;
        }

        var controlsStartX = rowRect.x + Mathf.Max(120f, width - reservedWidth);
        var checkboxRect = new Rect(rowRect.x, rowRect.y, Mathf.Max(120f, controlsStartX - rowRect.x - 6f), 28f);

        var label = abilityDef?.label ?? GenText.SplitCamelCase(spec.AbilityDefName);
        if (missing) {
            label += " (missing)";
        }

        Widgets.CheckboxLabeled(checkboxRect, label, ref enabled);
        Settings.SetEnabled(spec.AbilityDefName, enabled);

        var category = Settings.GetCategory(spec.AbilityDefName);
        var categoryButtonRect = new Rect(controlsStartX, rowRect.y, categoryWidth, 28f);
        if (Widgets.ButtonText(categoryButtonRect, BottledAbilityCatalog.CategoryLabel(category))) {
            OpenCategoryMenu(spec.AbilityDefName);
        }

        var charges = Settings.GetCharges(spec.AbilityDefName);
        var chargeButtonRect = new Rect(categoryButtonRect.xMax + controlsGap, rowRect.y, chargeButtonWidth, 28f);
        if (Widgets.ButtonText(chargeButtonRect, $"x{charges}")) {
            OpenChargesSlider(spec.AbilityDefName, charges);
        }

        y += 30f;
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
            val => $"Charges: x{val}",
            BottledAbilitySettings.MinCharges,
            BottledAbilitySettings.MaxCharges,
            delegate(int selected) {
                Settings.SetCharges(abilityDefName, selected);
                WriteSettings();
            },
            currentCharges));
    }

    private void OpenColorPicker(BottledAbilityCategory category) {
        Find.WindowStack.Add(new Dialog_ChooseColor(
            $"{BottledAbilityCatalog.CategoryLabel(category)} color",
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
}