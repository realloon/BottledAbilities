using HarmonyLib;
using RimWorld;
using Verse;

namespace BottledAbilities;

public static class DefGenerator {
    private static readonly System.Reflection.MethodInfo? RemoveThingDefMethod =
        AccessTools.Method(typeof(DefDatabase<ThingDef>), "Remove");

    public static void GenerateOrUpdate(bool hotReload) {
        var settings = BottledAbilitiesMod.Settings;
        var specs = BottledAbilityCatalog.GetAvailableSpecs();
        settings.InitializeIfNeeded(specs);

        var jarCategory = DefDatabase<ThingCategoryDef>.GetNamed(BottledAbilityCatalog.JarThingCategoryDefName, false);
        if (jarCategory is null) {
            Log.ErrorOnce(
                $"[BottledAbilities] Missing thing category def '{BottledAbilityCatalog.JarThingCategoryDefName}'.",
                176512301);
        }

        foreach (var spec in specs) {
            var jarDefName = spec.JarDefName;
            var abilityDef = DefDatabase<AbilityDef>.GetNamedSilentFail(spec.AbilityDefName);
            var enabled = settings.IsEnabled(spec.AbilityDefName);
            var existing = DefDatabase<ThingDef>.GetNamedSilentFail(jarDefName);

            if (!enabled || abilityDef is null) {
                if (existing is not null) {
                    RemoveThingDef(existing);
                }

                continue;
            }

            var color = settings.GetColor(settings.GetCategory(spec.AbilityDefName));
            var charges = settings.GetCharges(spec.AbilityDefName);

            if (existing is null) {
                var generated = CreateJarDef(jarCategory, abilityDef, spec, color, charges);
                RimWorld.DefGenerator.AddImpliedDef(generated, hotReload);
            } else {
                ConfigureJarDef(existing, jarCategory, abilityDef, color, charges);
            }
        }
    }

    private static void RemoveThingDef(ThingDef def) {
        if (RemoveThingDefMethod is null) {
            Log.ErrorOnce("[BottledAbilities] Unable to remove disabled jar defs. Reflection lookup failed.",
                1394478412);
            return;
        }

        RemoveThingDefMethod.Invoke(null, [def]);
    }

    private static ThingDef CreateJarDef(ThingCategoryDef? jarCategory, AbilityDef abilityDef, BottledAbilitySpec spec,
        UnityEngine.Color color, int charges) {
        var def = new ThingDef {
            defName = spec.JarDefName,
            shortHash = 0,
            index = ushort.MaxValue,
            debugRandomId = (ushort)Rand.RangeInclusive(0, 65535)
        };

        ConfigureJarDef(def, jarCategory, abilityDef, color, charges);

        return def;
    }

    private static void ConfigureJarDef(ThingDef def, ThingCategoryDef? jarCategory, AbilityDef abilityDef,
        UnityEngine.Color color, int charges) {
        ApplyBaseProperties(def, jarCategory);

        var abilityLabel = abilityDef.label.NullOrEmpty()
            ? GenText.SplitCamelCase(abilityDef.defName)
            : abilityDef.label;

        def.label = "VortexBA_GeneratedJarLabel".Translate(abilityLabel);
        def.description = "VortexBA_GeneratedJarDescription".Translate(abilityLabel.CapitalizeFirst());
        def.graphicData.color = color;
        def.ingestible.outcomeDoers = [
            new IngestionOutcomeDoer_GiveBottledAbility {
                abilityDef = abilityDef,
                charges = charges
            }
        ];
        def.descriptionHyperlinks = [abilityDef];
        def.ClearCachedData();
    }

    private static void ApplyBaseProperties(ThingDef def, ThingCategoryDef? jarCategory) {
        def.modContentPack = BottledAbilitiesMod.ContentPack;
        def.thingClass = typeof(ThingWithComps);
        def.category = ThingCategory.Item;
        def.drawerType = DrawerType.MapMeshOnly;
        def.useHitPoints = true;
        def.healthAffectsPrice = false;
        def.selectable = true;
        def.alwaysHaulable = true;
        def.rotatable = false;
        def.burnableByRecipe = true;
        def.pathCost = 14;
        def.altitudeLayer = AltitudeLayer.Item;
        def.stackLimit = 6;
        def.techLevel = TechLevel.Industrial;
        def.drawGUIOverlay = true;
        def.resourceReadoutPriority = ResourceCountPriority.Last;
        def.allowedArchonexusCount = -1;

        def.thingCategories = [];
        if (jarCategory is not null) {
            def.thingCategories.Add(jarCategory);
        }

        def.tradeTags = ["ExoticMisc"];
        def.thingSetMakerTags = ["RewardStandardMidFreq", "SkillNeurotrainer"];
        def.statBases = BuildBaseStatBases();
        def.comps = BuildBaseComps();
        def.graphicData = BuildBaseGraphicData();
        def.ingestible = BuildBaseIngestible();
    }

    #region Helper

    private static List<StatModifier> BuildBaseStatBases() {
        return [
            new StatModifier { stat = StatDefOf.MaxHitPoints, value = 50f },
            new StatModifier { stat = StatDefOf.Flammability, value = 1.0f },
            new StatModifier { stat = StatDefOf.DeteriorationRate, value = 2f },
            new StatModifier { stat = StatDefOf.Beauty, value = -4f },
            new StatModifier { stat = StatDefOf.MarketValue, value = 200f },
            new StatModifier { stat = StatDefOf.Mass, value = 0.1f }
        ];
    }

    private static List<CompProperties> BuildBaseComps() {
        return [
            new CompProperties_Forbiddable(),
            new CompProperties_BottledAbility()
        ];
    }

    private static GraphicData BuildBaseGraphicData() {
        return new GraphicData {
            texPath = "PortableAbility/Jars",
            graphicClass = typeof(Graphic_StackCount),
            shaderType = ShaderTypeDefOf.CutoutComplex
        };
    }

    private static IngestibleProperties BuildBaseIngestible() {
        return new IngestibleProperties {
            preferability = FoodPreferability.NeverForNutrition,
            maxNumToIngestAtOnce = 1,
            defaultNumToIngestAtOnce = 1,
            chairSearchRadius = 8f,
            foodType = FoodTypeFlags.Fluid | FoodTypeFlags.Processed,
            baseIngestTicks = 100,
            outcomeDoers = []
        };
    }

    #endregion
}