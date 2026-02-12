using RimWorld;
using UnityEngine;
using Verse;

namespace BottledAbilities;

public sealed class BottledAbilitySpec {
    public BottledAbilitySpec(string abilityDefName, string packageId, BottledAbilityCategory defaultCategory, int defaultCharges = 1) {
        AbilityDefName = abilityDefName;
        PackageId = packageId;
        DefaultCategory = defaultCategory;
        DefaultCharges = defaultCharges;
    }

    public string AbilityDefName { get; }
    public string PackageId { get; }
    public BottledAbilityCategory DefaultCategory { get; }
    public int DefaultCharges { get; }

    public string JarDefName => $"VortexBA_{AbilityDefName}";
}

public static class BottledAbilityCatalog {
    public const string JarThingCategoryDefName = "VortexBA_BottledAbilities";

    private static readonly IReadOnlyList<string> PreferredPackageOrder = [
        "ludeon.rimworld.royalty",
        "ludeon.rimworld.ideology",
        "ludeon.rimworld.biotech",
        "ludeon.rimworld.odyssey",
        "ludeon.rimworld.anomaly",
        "ludeon.rimworld"
    ];

    private static readonly Dictionary<string, (BottledAbilityCategory Category, int Charges)> KnownDefaults = new(StringComparer.Ordinal) {
        ["Painblock"] = (BottledAbilityCategory.Support, 1),
        ["Stun"] = (BottledAbilityCategory.Control, 1),
        ["Burden"] = (BottledAbilityCategory.Control, 1),
        ["BlindingPulse"] = (BottledAbilityCategory.Control, 1),
        ["EntropyDump"] = (BottledAbilityCategory.Utility, 1),
        ["Beckon"] = (BottledAbilityCategory.Control, 1),
        ["VertigoPulse"] = (BottledAbilityCategory.Control, 1),
        ["ChaosSkip"] = (BottledAbilityCategory.Mobility, 1),
        ["Skip"] = (BottledAbilityCategory.Mobility, 1),
        ["Wallraise"] = (BottledAbilityCategory.Utility, 1),
        ["Smokepop"] = (BottledAbilityCategory.Support, 1),
        ["Focus"] = (BottledAbilityCategory.Support, 1),
        ["Berserk"] = (BottledAbilityCategory.Control, 1),
        ["Invisibility"] = (BottledAbilityCategory.Support, 1),
        ["BerserkPulse"] = (BottledAbilityCategory.Control, 1),
        ["ManhunterPulse"] = (BottledAbilityCategory.Control, 1),
        ["MassChaosSkip"] = (BottledAbilityCategory.Mobility, 1),
        ["Waterskip"] = (BottledAbilityCategory.Mobility, 1),
        ["Flashstorm"] = (BottledAbilityCategory.Offense, 1),
        ["BulletShield"] = (BottledAbilityCategory.Support, 1),
        ["Speech"] = (BottledAbilityCategory.Utility, 1),
        ["SolarPinhole"] = (BottledAbilityCategory.Utility, 1),
        ["WordOfTrust"] = (BottledAbilityCategory.Utility, 1),
        ["WordOfJoy"] = (BottledAbilityCategory.Support, 1),
        ["WordOfLove"] = (BottledAbilityCategory.Utility, 1),
        ["WordOfSerenity"] = (BottledAbilityCategory.Control, 1),
        ["WordOfInspiration"] = (BottledAbilityCategory.Support, 1),
        ["Farskip"] = (BottledAbilityCategory.Mobility, 1),
        ["Neuroquake"] = (BottledAbilityCategory.Offense, 1),
        ["Chunkskip"] = (BottledAbilityCategory.Utility, 1),

        ["LeaderSpeech"] = (BottledAbilityCategory.Utility, 1),
        ["Trial"] = (BottledAbilityCategory.Control, 1),
        ["ConversionRitual"] = (BottledAbilityCategory.Control, 1),
        ["WorkDrive"] = (BottledAbilityCategory.Support, 1),
        ["CombatCommand"] = (BottledAbilityCategory.Offense, 1),
        ["Convert"] = (BottledAbilityCategory.Control, 1),
        ["PreachHealth"] = (BottledAbilityCategory.Support, 1),
        ["Reassure"] = (BottledAbilityCategory.Support, 1),
        ["Counsel"] = (BottledAbilityCategory.Support, 1),
        ["MarksmanCommand"] = (BottledAbilityCategory.Offense, 1),
        ["BerserkTrance"] = (BottledAbilityCategory.Offense, 1),
        ["ResearchCommand"] = (BottledAbilityCategory.Utility, 1),
        ["FarmingCommand"] = (BottledAbilityCategory.Utility, 1),
        ["ProductionCommand"] = (BottledAbilityCategory.Utility, 1),
        ["MiningCommand"] = (BottledAbilityCategory.Utility, 1),
        ["AnimalCalm"] = (BottledAbilityCategory.Control, 1),
        ["ImmunityDrive"] = (BottledAbilityCategory.Support, 1),

        ["Bloodfeed"] = (BottledAbilityCategory.Support, 1),
        ["Coagulate"] = (BottledAbilityCategory.Support, 1),
        ["ReimplantXenogerm"] = (BottledAbilityCategory.Tech, 1),
        ["PiercingSpine"] = (BottledAbilityCategory.Offense, 1),
        ["AcidSpray"] = (BottledAbilityCategory.Offense, 1),
        ["FoamSpray"] = (BottledAbilityCategory.Control, 1),
        ["FireSpew"] = (BottledAbilityCategory.Offense, 1),
        ["Longjump"] = (BottledAbilityCategory.Mobility, 1),
        ["AnimalWarcall"] = (BottledAbilityCategory.Control, 1),
        ["RemoteRepair"] = (BottledAbilityCategory.Tech, 1),
        ["RemoteShield"] = (BottledAbilityCategory.Support, 1),

        ["CallMechanoids"] = (BottledAbilityCategory.Tech, 1),
        ["CallDropPods"] = (BottledAbilityCategory.Tech, 1),
        ["DeactivateMechanoid"] = (BottledAbilityCategory.Tech, 1),
        ["LaunchFragGrenade"] = (BottledAbilityCategory.Offense, 1),
        ["LaunchEMPShell"] = (BottledAbilityCategory.Tech, 1),
        ["LaunchSmokeShell"] = (BottledAbilityCategory.Utility, 1),
        ["LaunchIncendiaryShell"] = (BottledAbilityCategory.Offense, 1),
        ["EMPPulse"] = (BottledAbilityCategory.Tech, 1),
        ["HellcatBurner"] = (BottledAbilityCategory.Offense, 1),
        ["IncineratorBurner"] = (BottledAbilityCategory.Offense, 1),

        ["UnnaturalHealing"] = (BottledAbilityCategory.Support, 1),
        ["ShapeFlesh"] = (BottledAbilityCategory.Support, 1),
        ["TransmuteSteel"] = (BottledAbilityCategory.Utility, 1),
        ["PsychicSlaughter"] = (BottledAbilityCategory.Offense, 1),
        ["ReleaseDeadlifeDust"] = (BottledAbilityCategory.Offense, 1),
        ["RevenantInvisibility"] = (BottledAbilityCategory.Support, 1),
        ["VoidTerror"] = (BottledAbilityCategory.Control, 1),
        ["GhoulFrenzy"] = (BottledAbilityCategory.Offense, 1),
        ["CorrosiveSpray"] = (BottledAbilityCategory.Offense, 1),
        ["MetalbloodInjection"] = (BottledAbilityCategory.Support, 1)
    };

    public static readonly IReadOnlyList<BottledAbilityCategory> OrderedCategories = new List<BottledAbilityCategory> {
        BottledAbilityCategory.Support,
        BottledAbilityCategory.Control,
        BottledAbilityCategory.Mobility,
        BottledAbilityCategory.Offense,
        BottledAbilityCategory.Utility,
        BottledAbilityCategory.Tech
    };

    public static IReadOnlyList<BottledAbilitySpec> GetAvailableSpecs() {
        var result = new List<BottledAbilitySpec>();

        foreach (var abilityDef in DefDatabase<AbilityDef>.AllDefsListForReading) {
            if (abilityDef is null || abilityDef.defName.NullOrEmpty()) continue;
            result.Add(BuildSpec(abilityDef));
        }

        result.Sort(CompareSpec);
        return result;
    }

    public static BottledAbilitySpec? FindSpec(string abilityDefName) {
        var abilityDef = DefDatabase<AbilityDef>.GetNamedSilentFail(abilityDefName);
        return abilityDef is null ? null : BuildSpec(abilityDef);
    }

    public static string CategoryLabel(BottledAbilityCategory category) {
        return category switch {
            BottledAbilityCategory.Support => "Support",
            BottledAbilityCategory.Control => "Control",
            BottledAbilityCategory.Mobility => "Mobility",
            BottledAbilityCategory.Offense => "Offense",
            BottledAbilityCategory.Utility => "Utility",
            BottledAbilityCategory.Tech => "Tech",
            _ => category.ToString()
        };
    }

    public static string PackageLabel(string packageId) {
        return packageId.ToLowerInvariant() switch {
            "ludeon.rimworld.royalty" => "Royalty",
            "ludeon.rimworld.ideology" => "Ideology",
            "ludeon.rimworld.biotech" => "Biotech",
            "ludeon.rimworld.odyssey" => "Odyssey",
            "ludeon.rimworld.anomaly" => "Anomaly",
            "ludeon.rimworld" => "Core",
            _ => packageId
        };
    }

    public static Color DefaultColor(BottledAbilityCategory category) {
        return category switch {
            BottledAbilityCategory.Support => new Color(0.22f, 0.78f, 0.42f),
            BottledAbilityCategory.Control => new Color(0.72f, 0.43f, 0.93f),
            BottledAbilityCategory.Mobility => new Color(0.24f, 0.78f, 0.96f),
            BottledAbilityCategory.Offense => new Color(0.94f, 0.34f, 0.22f),
            BottledAbilityCategory.Utility => new Color(0.96f, 0.79f, 0.24f),
            BottledAbilityCategory.Tech => new Color(0.44f, 0.66f, 0.96f),
            _ => Color.white
        };
    }

    public static bool DefaultEnabled() {
        return true;
    }

    private static BottledAbilitySpec BuildSpec(AbilityDef abilityDef) {
        var abilityDefName = abilityDef.defName;
        var packageId = abilityDef.modContentPack?.PackageId ?? "unknown";

        if (KnownDefaults.TryGetValue(abilityDefName, out var defaults)) {
            return new BottledAbilitySpec(abilityDefName, packageId, defaults.Category, defaults.Charges);
        }

        return new BottledAbilitySpec(
            abilityDefName,
            packageId,
            InferCategory(abilityDef),
            1);
    }

    private static BottledAbilityCategory InferCategory(AbilityDef abilityDef) {
        var defName = abilityDef.defName.ToLowerInvariant();

        if (ContainsAny(defName, "skip", "jump", "teleport", "longjump", "farskip", "blink", "dash")) {
            return BottledAbilityCategory.Mobility;
        }

        if (ContainsAny(defName, "shield", "heal", "coagulate", "immunity", "repair", "invis", "protect", "focus", "reassure", "counsel")) {
            return BottledAbilityCategory.Support;
        }

        if (ContainsAny(defName, "stun", "burden", "berserk", "calm", "convert", "trial", "terror", "disable", "deactivate", "serenity")) {
            return BottledAbilityCategory.Control;
        }

        if (ContainsAny(defName, "mech", "emp", "xenogerm", "remote", "transmute", "call", "signal", "hack")) {
            return BottledAbilityCategory.Tech;
        }

        if (abilityDef.ai_IsOffensive || abilityDef.hostile) {
            return BottledAbilityCategory.Offense;
        }

        return BottledAbilityCategory.Utility;
    }

    private static int CompareSpec(BottledAbilitySpec left, BottledAbilitySpec right) {
        var leftOrder = PackageOrder(left.PackageId);
        var rightOrder = PackageOrder(right.PackageId);
        if (leftOrder != rightOrder) return leftOrder.CompareTo(rightOrder);

        var packageCmp = string.Compare(left.PackageId, right.PackageId, StringComparison.OrdinalIgnoreCase);
        if (packageCmp != 0) return packageCmp;

        return string.Compare(left.AbilityDefName, right.AbilityDefName, StringComparison.OrdinalIgnoreCase);
    }

    private static int PackageOrder(string packageId) {
        var normalized = packageId.ToLowerInvariant();
        for (var i = 0; i < PreferredPackageOrder.Count; i++) {
            if (PreferredPackageOrder[i] == normalized) return i;
        }

        return 1000;
    }

    private static bool ContainsAny(string value, params string[] keywords) {
        foreach (var keyword in keywords) {
            if (value.Contains(keyword)) return true;
        }

        return false;
    }
}
