namespace BottledAbilities;

public sealed class BottledAbilitySpec(
    string abilityDefName,
    string packageId,
    AbilityCategory defaultCategory,
    int defaultCharges = 1,
    bool defaultEnabled = true) {
    public string AbilityDefName { get; } = abilityDefName;
    public string PackageId { get; } = packageId;
    public AbilityCategory DefaultCategory { get; } = defaultCategory;
    public int DefaultCharges { get; } = defaultCharges;
    public bool DefaultEnabled { get; } = defaultEnabled;

    public string JarDefName => $"VortexBA_{AbilityDefName}";
}
