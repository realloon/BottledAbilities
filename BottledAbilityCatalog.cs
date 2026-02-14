using UnityEngine;
using RimWorld;
using Verse;

namespace BottledAbilities;

public static class BottledAbilityCatalog {
    public const string JarThingCategoryDefName = "VortexBA_BottledAbilities";
    private const bool UnknownDefaultEnabled = false;

    private static readonly Dictionary<string, (AbilityCategory Category, int Charges, bool Enabled)> KnownDefaults =
        new(StringComparer.Ordinal) {
            ["Painblock"] = (AbilityCategory.Support, 1, true),
            ["Stun"] = (AbilityCategory.Control, 1, true),
            ["Burden"] = (AbilityCategory.Control, 1, true),
            ["BlindingPulse"] = (AbilityCategory.Control, 1, true),
            ["EntropyDump"] = (AbilityCategory.Utility, 1, true),
            ["Beckon"] = (AbilityCategory.Control, 1, true),
            ["VertigoPulse"] = (AbilityCategory.Control, 1, true),
            ["ChaosSkip"] = (AbilityCategory.Mobility, 1, true),
            ["Skip"] = (AbilityCategory.Mobility, 1, true),
            ["Wallraise"] = (AbilityCategory.Utility, 1, true),
            ["Smokepop"] = (AbilityCategory.Support, 1, true),
            ["Focus"] = (AbilityCategory.Support, 1, true),
            ["Berserk"] = (AbilityCategory.Control, 1, true),
            ["Invisibility"] = (AbilityCategory.Support, 1, true),
            ["BerserkPulse"] = (AbilityCategory.Control, 1, true),
            ["ManhunterPulse"] = (AbilityCategory.Control, 1, true),
            ["MassChaosSkip"] = (AbilityCategory.Mobility, 1, true),
            ["Waterskip"] = (AbilityCategory.Mobility, 1, true),
            ["Flashstorm"] = (AbilityCategory.Offense, 1, true),
            ["BulletShield"] = (AbilityCategory.Support, 1, true),
            ["Speech"] = (AbilityCategory.Utility, 1, true),
            ["SolarPinhole"] = (AbilityCategory.Utility, 1, true),
            ["WordOfTrust"] = (AbilityCategory.Utility, 1, true),
            ["WordOfJoy"] = (AbilityCategory.Support, 1, true),
            ["WordOfLove"] = (AbilityCategory.Utility, 1, true),
            ["WordOfSerenity"] = (AbilityCategory.Control, 1, true),
            ["WordOfInspiration"] = (AbilityCategory.Support, 1, true),
            ["Farskip"] = (AbilityCategory.Mobility, 1, true),
            ["Neuroquake"] = (AbilityCategory.Offense, 1, true),
            ["Chunkskip"] = (AbilityCategory.Utility, 1, true),
            ["LeaderSpeech"] = (AbilityCategory.Utility, 1, false),
            ["Trial"] = (AbilityCategory.Control, 1, false),
            ["ConversionRitual"] = (AbilityCategory.Control, 1, false),
            ["WorkDrive"] = (AbilityCategory.Support, 1, false),
            ["CombatCommand"] = (AbilityCategory.Offense, 1, false),
            ["Convert"] = (AbilityCategory.Control, 1, false),
            ["PreachHealth"] = (AbilityCategory.Support, 1, false),
            ["Reassure"] = (AbilityCategory.Support, 1, false),
            ["Counsel"] = (AbilityCategory.Support, 1, false),
            ["MarksmanCommand"] = (AbilityCategory.Offense, 1, false),
            ["BerserkTrance"] = (AbilityCategory.Offense, 1, false),
            ["ResearchCommand"] = (AbilityCategory.Utility, 1, false),
            ["FarmingCommand"] = (AbilityCategory.Utility, 1, false),
            ["ProductionCommand"] = (AbilityCategory.Utility, 1, false),
            ["MiningCommand"] = (AbilityCategory.Utility, 1, false),
            ["AnimalCalm"] = (AbilityCategory.Control, 1, false),
            ["ImmunityDrive"] = (AbilityCategory.Support, 1, false),
            ["Bloodfeed"] = (AbilityCategory.Support, 1, true),
            ["Coagulate"] = (AbilityCategory.Support, 1, true),
            ["ReimplantXenogerm"] = (AbilityCategory.Tech, 1, false),
            ["PiercingSpine"] = (AbilityCategory.Offense, 1, false),
            ["AcidSpray"] = (AbilityCategory.Offense, 1, true),
            ["FoamSpray"] = (AbilityCategory.Control, 1, true),
            ["FireSpew"] = (AbilityCategory.Offense, 1, true),
            ["Longjump"] = (AbilityCategory.Mobility, 1, true),
            ["AnimalWarcall"] = (AbilityCategory.Control, 1, true),
            ["RemoteRepair"] = (AbilityCategory.Tech, 1, false),
            ["RemoteShield"] = (AbilityCategory.Support, 1, false),
            ["CallMechanoids"] = (AbilityCategory.Tech, 1, false),
            ["CallDropPods"] = (AbilityCategory.Tech, 1, false),
            ["DeactivateMechanoid"] = (AbilityCategory.Tech, 1, false),
            ["LaunchFragGrenade"] = (AbilityCategory.Offense, 1, false),
            ["LaunchEMPShell"] = (AbilityCategory.Tech, 1, false),
            ["LaunchSmokeShell"] = (AbilityCategory.Utility, 1, false),
            ["LaunchIncendiaryShell"] = (AbilityCategory.Offense, 1, false),
            ["EMPPulse"] = (AbilityCategory.Tech, 1, false),
            ["HellcatBurner"] = (AbilityCategory.Offense, 1, false),
            ["IncineratorBurner"] = (AbilityCategory.Offense, 1, false),
            ["UnnaturalHealing"] = (AbilityCategory.Support, 1, true),
            ["ShapeFlesh"] = (AbilityCategory.Support, 1, false),
            ["TransmuteSteel"] = (AbilityCategory.Utility, 1, true),
            ["PsychicSlaughter"] = (AbilityCategory.Offense, 1, true),
            ["ReleaseDeadlifeDust"] = (AbilityCategory.Offense, 1, false),
            ["RevenantInvisibility"] = (AbilityCategory.Support, 1, false),
            ["VoidTerror"] = (AbilityCategory.Control, 1, false),
            ["GhoulFrenzy"] = (AbilityCategory.Offense, 1, true),
            ["CorrosiveSpray"] = (AbilityCategory.Offense, 1, true),
            ["MetalbloodInjection"] = (AbilityCategory.Support, 1, true)
        };

    public static readonly IReadOnlyList<AbilityCategory> OrderedCategories = new List<AbilityCategory> {
        AbilityCategory.Support,
        AbilityCategory.Control,
        AbilityCategory.Mobility,
        AbilityCategory.Offense,
        AbilityCategory.Utility,
        AbilityCategory.Tech
    };

    public static IReadOnlyList<BottledAbilitySpec> GetAvailableSpecs() {
        return DefDatabase<AbilityDef>.AllDefsListForReading
            .Where(abilityDef => !abilityDef.defName.NullOrEmpty())
            .Select(BuildSpec)
            .OrderBy(x => PackageLabel(x.PackageId), StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.AbilityDefName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static BottledAbilitySpec? FindSpec(string abilityDefName) {
        var abilityDef = DefDatabase<AbilityDef>.GetNamedSilentFail(abilityDefName);
        return abilityDef is null ? null : BuildSpec(abilityDef);
    }

    public static string CategoryLabel(AbilityCategory category) {
        return category switch {
            AbilityCategory.Support => "VortexBA_Category_Support".Translate(),
            AbilityCategory.Control => "VortexBA_Category_Control".Translate(),
            AbilityCategory.Mobility => "VortexBA_Category_Mobility".Translate(),
            AbilityCategory.Offense => "VortexBA_Category_Offense".Translate(),
            AbilityCategory.Utility => "VortexBA_Category_Utility".Translate(),
            AbilityCategory.Tech => "VortexBA_Category_Tech".Translate(),
            _ => category.ToString()
        };
    }

    public static string PackageLabel(string packageId) {
        var expansionDef = ModLister.GetExpansionWithIdentifier(packageId);

        if (expansionDef?.label is not null) {
            return expansionDef.label;
        }

        return ModLister.GetModWithIdentifier(packageId)?.Name ?? packageId;
    }

    public static Color DefaultColor(AbilityCategory category) {
        return category switch {
            AbilityCategory.Support => new Color(0.22f, 0.78f, 0.42f),
            AbilityCategory.Control => new Color(0.72f, 0.43f, 0.93f),
            AbilityCategory.Mobility => new Color(0.24f, 0.78f, 0.96f),
            AbilityCategory.Offense => new Color(0.94f, 0.34f, 0.22f),
            AbilityCategory.Utility => new Color(0.96f, 0.79f, 0.24f),
            AbilityCategory.Tech => new Color(0.44f, 0.66f, 0.96f),
            _ => Color.white
        };
    }

    private static BottledAbilitySpec BuildSpec(AbilityDef abilityDef) {
        var abilityDefName = abilityDef.defName;
        var packageId = abilityDef.modContentPack!.PackageId;

        if (KnownDefaults.TryGetValue(abilityDefName, out var defaults)) {
            return new BottledAbilitySpec(
                abilityDefName,
                packageId,
                defaults.Category,
                defaults.Charges,
                defaults.Enabled);
        }

        return new BottledAbilitySpec(
            abilityDefName,
            packageId,
            AbilityCategory.Utility,
            defaultEnabled: UnknownDefaultEnabled);
    }
}