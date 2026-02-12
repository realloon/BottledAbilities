using RimWorld;
using UnityEngine;
using Verse;

namespace BottledAbilities;

public sealed class BottledAbilitiesMod : Mod {
    private static readonly List<Color> ColorPalette = BuildColorPalette();

    private Vector2 scrollPosition;
    private readonly Dictionary<string, string> chargeBuffers = new(StringComparer.Ordinal);

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

        var viewRect = new Rect(0f, 0f, inRect.width - 18f, CalculateViewHeight(inRect.width - 18f, specs));
        Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);

        var y = 0f;
        DrawIntro(ref y, viewRect.width);
        DrawPresetButtons(ref y, viewRect.width, specs);
        DrawCategoryColors(ref y, viewRect.width);
        DrawAbilityOptions(ref y, viewRect.width, specs);

        Widgets.EndScrollView();
    }

    private void DrawIntro(ref float y, float width) {
        var hintRect = new Rect(0f, y, width, 32f);
        Widgets.Label(hintRect, "Changes apply after restarting the game. Disabled jars are removed on startup.");
        y += 36f;

        var activePreset = Settings.IsCustomPreset ? "Custom" : Settings.activePreset;
        var presetRect = new Rect(0f, y, width, 28f);
        Widgets.Label(presetRect, $"Active preset: {activePreset}");
        y += 34f;
    }

    private void DrawPresetButtons(ref float y, float width, IReadOnlyList<BottledAbilitySpec> specs) {
        var colWidth = (width - 16f) / 3f;

        if (Widgets.ButtonText(new Rect(0f, y, colWidth, 28f), "Apply Balanced")) {
            Settings.ApplyPreset(BottledAbilityPresetKind.Balanced, specs);
            WriteSettings();
        }

        if (Widgets.ButtonText(new Rect(colWidth + 8f, y, colWidth, 28f), "Apply Combat")) {
            Settings.ApplyPreset(BottledAbilityPresetKind.Combat, specs);
            WriteSettings();
        }

        if (Widgets.ButtonText(new Rect((colWidth + 8f) * 2f, y, colWidth, 28f), "Apply Utility")) {
            Settings.ApplyPreset(BottledAbilityPresetKind.Utility, specs);
            WriteSettings();
        }

        y += 38f;
    }

    private void DrawCategoryColors(ref float y, float width) {
        Widgets.DrawLineHorizontal(0f, y, width);
        y += 6f;

        Widgets.Label(new Rect(0f, y, width, 24f), "Category colors");
        y += 26f;

        foreach (var category in BottledAbilityCatalog.OrderedCategories) {
            var rowRect = new Rect(0f, y, width, 28f);
            var labelRect = new Rect(rowRect.x, rowRect.y + 5f, 180f, 22f);
            Widgets.Label(labelRect, BottledAbilityCatalog.CategoryLabel(category));

            var colorRect = new Rect(rowRect.x + 190f, rowRect.y + 4f, 22f, 22f);
            Widgets.DrawBoxSolid(colorRect, Settings.GetColor(category));
            Widgets.DrawBox(colorRect);

            if (Widgets.ButtonText(new Rect(rowRect.x + 220f, rowRect.y, 130f, 28f), "Change color")) {
                OpenColorPicker(category);
            }

            y += 30f;
        }

        y += 6f;
    }

    private void DrawAbilityOptions(ref float y, float width, IReadOnlyList<BottledAbilitySpec> specs) {
        Widgets.DrawLineHorizontal(0f, y, width);
        y += 6f;
        Widgets.Label(new Rect(0f, y, width, 24f), "Ability jars");
        y += 28f;

        var grouped = specs
            .GroupBy(x => x.PackageId)
            .OrderBy(group => PackageSortOrder(group.Key))
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var group in grouped) {
            var packageName = BottledAbilityCatalog.PackageLabel(group.Key);
            Widgets.Label(new Rect(0f, y, width, 24f), packageName);
            y += 24f;

            foreach (var spec in group) {
                DrawAbilityRow(spec, ref y, width);
            }

            y += 6f;
        }
    }

    private void DrawAbilityRow(BottledAbilitySpec spec, ref float y, float width) {
        var rowRect = new Rect(0f, y, width, 28f);
        var abilityDef = DefDatabase<AbilityDef>.GetNamedSilentFail(spec.AbilityDefName);
        var missing = abilityDef is null;

        var enabled = Settings.IsEnabled(spec.AbilityDefName);
        var checkboxRect = new Rect(rowRect.x, rowRect.y, width - 280f, 28f);

        var label = abilityDef?.label ?? GenText.SplitCamelCase(spec.AbilityDefName);
        if (missing) {
            label += " (missing)";
        }

        Widgets.CheckboxLabeled(checkboxRect, label, ref enabled);
        Settings.SetEnabled(spec.AbilityDefName, enabled);

        var category = Settings.GetCategory(spec.AbilityDefName);

        var swatchRect = new Rect(rowRect.x + width - 272f, rowRect.y + 4f, 22f, 22f);
        Widgets.DrawBoxSolid(swatchRect, Settings.GetColor(category));
        Widgets.DrawBox(swatchRect);

        var categoryButtonRect = new Rect(rowRect.x + width - 242f, rowRect.y, 150f, 28f);
        if (Widgets.ButtonText(categoryButtonRect, BottledAbilityCatalog.CategoryLabel(category))) {
            OpenCategoryMenu(spec.AbilityDefName);
        }

        var charges = Settings.GetCharges(spec.AbilityDefName);
        if (!chargeBuffers.TryGetValue(spec.AbilityDefName, out var chargeBuffer) || chargeBuffer.NullOrEmpty()) {
            chargeBuffer = charges.ToString();
        }

        Widgets.Label(new Rect(rowRect.x + width - 86f, rowRect.y + 4f, 14f, 22f), "x");
        Widgets.TextFieldNumeric(new Rect(rowRect.x + width - 68f, rowRect.y + 4f, 64f, 22f),
            ref charges, ref chargeBuffer, 1f, 999f);
        chargeBuffers[spec.AbilityDefName] = chargeBuffer;
        Settings.SetCharges(spec.AbilityDefName, charges);

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

    private static float CalculateViewHeight(float width, IReadOnlyList<BottledAbilitySpec> specs) {
        var headerHeight = 36f + 34f + 38f;
        var categoryHeader = 6f + 26f + 6f;
        var categoryRows = BottledAbilityCatalog.OrderedCategories.Count * 30f;

        var grouped = specs
            .GroupBy(x => x.PackageId)
            .ToList();

        var abilityHeader = 6f + 28f;
        var groupHeaders = grouped.Count * 24f;
        var abilityRows = specs.Count * 30f;
        var groupSpacing = grouped.Count * 6f;

        var estimated = headerHeight + categoryHeader + categoryRows + abilityHeader + groupHeaders + abilityRows + groupSpacing + 32f;
        return Mathf.Max(estimated, 400f);
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
