using UnityEngine;
using RimWorld;
using Verse;

namespace BottledAbilities;

public static class BottledAbilityCatalog {
    public const string JarThingCategoryDefName = "VortexBA_BottledAbilities";

    public const bool IsDefaultEnabled = true;

    private static readonly Dictionary<string, (AbilityCategory Category, int Charges)> KnownDefaults =
        new(StringComparer.Ordinal) {
            ["Painblock"] = (AbilityCategory.Support, 1),
            ["Stun"] = (AbilityCategory.Control, 1),
            ["Burden"] = (AbilityCategory.Control, 1),
            ["BlindingPulse"] = (AbilityCategory.Control, 1),
            ["EntropyDump"] = (AbilityCategory.Utility, 1),
            ["Beckon"] = (AbilityCategory.Control, 1),
            ["VertigoPulse"] = (AbilityCategory.Control, 1),
            ["ChaosSkip"] = (AbilityCategory.Mobility, 1),
            ["Skip"] = (AbilityCategory.Mobility, 1),
            ["Wallraise"] = (AbilityCategory.Utility, 1),
            ["Smokepop"] = (AbilityCategory.Support, 1),
            ["Focus"] = (AbilityCategory.Support, 1),
            ["Berserk"] = (AbilityCategory.Control, 1),
            ["Invisibility"] = (AbilityCategory.Support, 1),
            ["BerserkPulse"] = (AbilityCategory.Control, 1),
            ["ManhunterPulse"] = (AbilityCategory.Control, 1),
            ["MassChaosSkip"] = (AbilityCategory.Mobility, 1),
            ["Waterskip"] = (AbilityCategory.Mobility, 1),
            ["Flashstorm"] = (AbilityCategory.Offense, 1),
            ["BulletShield"] = (AbilityCategory.Support, 1),
            ["Speech"] = (AbilityCategory.Utility, 1),
            ["SolarPinhole"] = (AbilityCategory.Utility, 1),
            ["WordOfTrust"] = (AbilityCategory.Utility, 1),
            ["WordOfJoy"] = (AbilityCategory.Support, 1),
            ["WordOfLove"] = (AbilityCategory.Utility, 1),
            ["WordOfSerenity"] = (AbilityCategory.Control, 1),
            ["WordOfInspiration"] = (AbilityCategory.Support, 1),
            ["Farskip"] = (AbilityCategory.Mobility, 1),
            ["Neuroquake"] = (AbilityCategory.Offense, 1),
            ["Chunkskip"] = (AbilityCategory.Utility, 1),

            ["LeaderSpeech"] = (AbilityCategory.Utility, 1),
            ["Trial"] = (AbilityCategory.Control, 1),
            ["ConversionRitual"] = (AbilityCategory.Control, 1),
            ["WorkDrive"] = (AbilityCategory.Support, 1),
            ["CombatCommand"] = (AbilityCategory.Offense, 1),
            ["Convert"] = (AbilityCategory.Control, 1),
            ["PreachHealth"] = (AbilityCategory.Support, 1),
            ["Reassure"] = (AbilityCategory.Support, 1),
            ["Counsel"] = (AbilityCategory.Support, 1),
            ["MarksmanCommand"] = (AbilityCategory.Offense, 1),
            ["BerserkTrance"] = (AbilityCategory.Offense, 1),
            ["ResearchCommand"] = (AbilityCategory.Utility, 1),
            ["FarmingCommand"] = (AbilityCategory.Utility, 1),
            ["ProductionCommand"] = (AbilityCategory.Utility, 1),
            ["MiningCommand"] = (AbilityCategory.Utility, 1),
            ["AnimalCalm"] = (AbilityCategory.Control, 1),
            ["ImmunityDrive"] = (AbilityCategory.Support, 1),

            ["Bloodfeed"] = (AbilityCategory.Support, 1),
            ["Coagulate"] = (AbilityCategory.Support, 1),
            ["ReimplantXenogerm"] = (AbilityCategory.Tech, 1),
            ["PiercingSpine"] = (AbilityCategory.Offense, 1),
            ["AcidSpray"] = (AbilityCategory.Offense, 1),
            ["FoamSpray"] = (AbilityCategory.Control, 1),
            ["FireSpew"] = (AbilityCategory.Offense, 1),
            ["Longjump"] = (AbilityCategory.Mobility, 1),
            ["AnimalWarcall"] = (AbilityCategory.Control, 1),
            ["RemoteRepair"] = (AbilityCategory.Tech, 1),
            ["RemoteShield"] = (AbilityCategory.Support, 1),

            ["CallMechanoids"] = (AbilityCategory.Tech, 1),
            ["CallDropPods"] = (AbilityCategory.Tech, 1),
            ["DeactivateMechanoid"] = (AbilityCategory.Tech, 1),
            ["LaunchFragGrenade"] = (AbilityCategory.Offense, 1),
            ["LaunchEMPShell"] = (AbilityCategory.Tech, 1),
            ["LaunchSmokeShell"] = (AbilityCategory.Utility, 1),
            ["LaunchIncendiaryShell"] = (AbilityCategory.Offense, 1),
            ["EMPPulse"] = (AbilityCategory.Tech, 1),
            ["HellcatBurner"] = (AbilityCategory.Offense, 1),
            ["IncineratorBurner"] = (AbilityCategory.Offense, 1),

            ["UnnaturalHealing"] = (AbilityCategory.Support, 1),
            ["ShapeFlesh"] = (AbilityCategory.Support, 1),
            ["TransmuteSteel"] = (AbilityCategory.Utility, 1),
            ["PsychicSlaughter"] = (AbilityCategory.Offense, 1),
            ["ReleaseDeadlifeDust"] = (AbilityCategory.Offense, 1),
            ["RevenantInvisibility"] = (AbilityCategory.Support, 1),
            ["VoidTerror"] = (AbilityCategory.Control, 1),
            ["GhoulFrenzy"] = (AbilityCategory.Offense, 1),
            ["CorrosiveSpray"] = (AbilityCategory.Offense, 1),
            ["MetalbloodInjection"] = (AbilityCategory.Support, 1)
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
            return new BottledAbilitySpec(abilityDefName, packageId, defaults.Category, defaults.Charges);
        }

        return new BottledAbilitySpec(
            abilityDefName,
            packageId,
            AbilityCategory.Utility);
    }
}
