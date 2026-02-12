using RimWorld;
using UnityEngine;
using Verse;

namespace BottledAbilities;

public sealed class BottledAbilitiesMod : Mod {
    private static readonly List<Color> ColorPalette = BuildColorPalette();
    private const float TabsAreaHeight = 36f;

    private enum SettingsTab {
        AbilityJars,
        CategoryColors
    }

    private SettingsTab activeTab = SettingsTab.AbilityJars;
    private string? selectedAbilityPackageId;
    private Vector2 abilityPackageListScrollPosition;
    private Vector2 abilityListScrollPosition;

    public static BottledAbilitySettings Settings { get; private set; } = new();

    public BottledAbilitiesMod(ModContentPack content) : base(content) {
        BottledAbilities.EnsurePatched();
        Settings = GetSettings<BottledAbilitySettings>();
        Settings.InitializeIfNeeded();
    }

    public override string SettingsCategory() {
        return "Bottled Abilities";
    }

    public override void DoSettingsWindowContents(Rect inRect) {
        var specs = BottledAbilityCatalog.GetAvailableSpecs();
        Settings.InitializeIfNeeded(specs);

        var y = 0f;
        DrawTabs(ref y, inRect.width);

        switch (activeTab) {
            case SettingsTab.AbilityJars:
                DrawPageHint(ref y, inRect.width);
                DrawAbilityOptions(ref y, inRect.width, inRect.height - y - 76f, specs);
                DrawResetAbilityDefaultsButton(ref y, inRect.width, specs);
                break;
            case SettingsTab.CategoryColors:
                DrawPageHint(ref y, inRect.width);
                DrawCategoryColors(ref y, inRect.width);
                DrawResetColorDefaultsButton(ref y, inRect.width);
                break;
        }
    }

    private void DrawTabs(ref float y, float width) {
        var tabs = new List<TabRecord> {
            new("Ability Jars", () => activeTab = SettingsTab.AbilityJars, activeTab == SettingsTab.AbilityJars),
            new("Category Colors", () => activeTab = SettingsTab.CategoryColors, activeTab == SettingsTab.CategoryColors)
        };

        // TabDrawer draws tabs at (baseRect.y - tabHeight), so offset the base rect down.
        var tabsBaseRect = new Rect(0f, y + TabDrawer.TabHeight, width, 1f);
        TabDrawer.DrawTabs(tabsBaseRect, tabs, 180f);
        y += TabsAreaHeight;
    }

    private void DrawPageHint(ref float y, float width) {
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
        y += 4f;

        Widgets.Label(new Rect(0f, y, width, 24f), "Category colors");
        y += 26f;

        foreach (var category in BottledAbilityCatalog.OrderedCategories) {
            var rowRect = new Rect(0f, y, width, 28f);
            var labelRect = new Rect(rowRect.x, rowRect.y + 5f, 180f, 22f);
            Widgets.Label(labelRect, BottledAbilityCatalog.CategoryLabel(category));

            var colorRect = new Rect(rowRect.x + 190f, rowRect.y + 4f, 22f, 22f);
            Widgets.DrawBoxSolid(colorRect, Settings.GetColor(category));

            if (Widgets.ButtonText(new Rect(rowRect.x + 220f, rowRect.y, 130f, 28f), "Change color")) {
                OpenColorPicker(category);
            }

            y += 30f;
        }

        y += 6f;
    }

    private void DrawAbilityOptions(ref float y, float width, float availableHeight, IReadOnlyList<BottledAbilitySpec> specs) {
        y += 4f;
        Widgets.Label(new Rect(0f, y, width, 24f), "Ability jars");
        y += 28f;

        var packageIds = GetOrderedPackageIds(specs);
        EnsureSelectedAbilityPackage(packageIds);

        var filteredSpecs = selectedAbilityPackageId is null
            ? []
            : specs.Where(x => string.Equals(x.PackageId, selectedAbilityPackageId, StringComparison.OrdinalIgnoreCase)).ToList();

        var leftColumnWidth = Mathf.Min(250f, Mathf.Max(200f, width * 0.30f));
        var columnGap = 14f;
        var rightColumnX = leftColumnWidth + columnGap;
        var rightColumnWidth = Mathf.Max(240f, width - leftColumnWidth - columnGap);
        var columnsHeight = Mathf.Max(220f, availableHeight);

        Widgets.Label(new Rect(0f, y, leftColumnWidth, 24f), "DLC / Mod");
        var selectedLabel = selectedAbilityPackageId is null
            ? "Abilities"
            : $"Abilities ({BottledAbilityCatalog.PackageLabel(selectedAbilityPackageId)})";
        Widgets.Label(new Rect(rightColumnX, y, rightColumnWidth, 24f), selectedLabel);
        y += 28f;

        DrawPackageSelectorColumn(0f, y, leftColumnWidth, columnsHeight, packageIds);
        DrawAbilityListColumn(rightColumnX, y, rightColumnWidth, columnsHeight, filteredSpecs);
        y += columnsHeight + 8f;
    }

    private void DrawPackageSelectorColumn(float x, float y, float width, float height, IReadOnlyList<string> packageIds) {
        var rowHeight = 28f;
        var rowGap = 2f;
        var contentRect = new Rect(x, y, width, height);

        var rowCount = Mathf.Max(1, packageIds.Count);
        var viewHeight = 4f + rowCount * (rowHeight + rowGap);
        var viewRect = new Rect(0f, 0f, contentRect.width - 4f, Mathf.Max(contentRect.height, viewHeight));
        Widgets.BeginScrollView(contentRect, ref abilityPackageListScrollPosition, viewRect, false);

        var rowY = 2f;
        if (packageIds.Count == 0) {
            Widgets.Label(new Rect(6f, rowY + 4f, viewRect.width - 12f, 22f), "No loaded mods.");
            Widgets.EndScrollView();
            return;
        }

        foreach (var packageId in packageIds) {
            var rowRect = new Rect(0f, rowY, viewRect.width - 2f, rowHeight);
            var isSelected = string.Equals(packageId, selectedAbilityPackageId, StringComparison.OrdinalIgnoreCase);
            if (isSelected) {
                Widgets.DrawHighlightSelected(rowRect);
            }
            else {
                Widgets.DrawHighlightIfMouseover(rowRect);
            }

            if (Widgets.ButtonInvisible(rowRect)) {
                selectedAbilityPackageId = packageId;
                abilityListScrollPosition = Vector2.zero;
            }

            var labelRect = new Rect(rowRect.x + 6f, rowRect.y + 5f, rowRect.width - 12f, 22f);
            Widgets.Label(labelRect, BottledAbilityCatalog.PackageLabel(packageId));
            rowY += rowHeight + rowGap;
        }
        Widgets.EndScrollView();
    }

    private void DrawAbilityListColumn(float x, float y, float width, float height, IReadOnlyList<BottledAbilitySpec> specs) {
        var contentRect = new Rect(x, y, width, height);

        var rows = Mathf.Max(1, specs.Count);
        var viewHeight = 4f + rows * 30f;
        var viewRect = new Rect(0f, 0f, contentRect.width - 16f, Mathf.Max(contentRect.height, viewHeight));
        Widgets.BeginScrollView(contentRect, ref abilityListScrollPosition, viewRect);

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
            selectedAbilityPackageId = null;
            return;
        }

        if (selectedAbilityPackageId is null) {
            selectedAbilityPackageId = packageIds[0];
            return;
        }

        foreach (var packageId in packageIds) {
            if (string.Equals(packageId, selectedAbilityPackageId, StringComparison.OrdinalIgnoreCase)) {
                return;
            }
        }

        selectedAbilityPackageId = packageIds[0];
    }

    private static List<string> GetOrderedPackageIds(IReadOnlyList<BottledAbilitySpec> specs) {
        return specs
            .Select(x => x.PackageId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(PackageSortOrder)
            .ThenBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void DrawAbilityRow(BottledAbilitySpec spec, float x, ref float y, float width) {
        var rowRect = new Rect(x, y, width, 28f);
        var abilityDef = DefDatabase<AbilityDef>.GetNamedSilentFail(spec.AbilityDefName);
        var missing = abilityDef is null;

        var enabled = Settings.IsEnabled(spec.AbilityDefName);
        var categoryWidth = 140f;
        var sliderWidth = 86f;
        var chargeWidth = 28f;
        var controlsGap = 10f;
        var reservedWidth = categoryWidth + controlsGap + sliderWidth + chargeWidth;

        if (reservedWidth > width * 0.55f) {
            categoryWidth = 116f;
            sliderWidth = 72f;
            reservedWidth = categoryWidth + controlsGap + sliderWidth + chargeWidth;
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
        var sliderRect = new Rect(categoryButtonRect.xMax + controlsGap, rowRect.y + 6f, sliderWidth, 20f);
        charges = Mathf.RoundToInt(Widgets.HorizontalSlider(sliderRect, charges, BottledAbilitySettings.MinCharges,
            BottledAbilitySettings.MaxCharges, false, null, "1", "9", 1f));
        Settings.SetCharges(spec.AbilityDefName, charges);
        Widgets.Label(new Rect(sliderRect.xMax + 2f, rowRect.y + 4f, chargeWidth, 22f), $"x{charges}");

        y += 30f;
    }

    private void OpenCategoryMenu(string abilityDefName) {
        var options = new List<FloatMenuOption>();

        foreach (var category in BottledAbilityCatalog.OrderedCategories) {
            var captured = category;
            options.Add(new FloatMenuOption(BottledAbilityCatalog.CategoryLabel(captured), delegate {
                Settings.SetCategory(abilityDefName, captured);
                WriteSettings();
            }));
        }

        Find.WindowStack.Add(new FloatMenu(options));
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
                foreach (var value in new[] { 0.55f, 0.75f, 0.95f }) {
                    colors.Add(Color.HSVToRGB(h, saturation, value));
                }
            }
        }

        return colors;
    }

    private static int PackageSortOrder(string packageId) {
        return packageId.ToLowerInvariant() switch {
            "ludeon.rimworld.royalty" => 0,
            "ludeon.rimworld.ideology" => 1,
            "ludeon.rimworld.biotech" => 2,
            "ludeon.rimworld.odyssey" => 3,
            "ludeon.rimworld.anomaly" => 4,
            "ludeon.rimworld" => 5,
            _ => 1000
        };
    }
}
