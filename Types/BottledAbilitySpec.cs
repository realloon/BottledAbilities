namespace BottledAbilities;

public sealed class BottledAbilitySpec(
    string abilityDefName,
    string packageId,
    BottledAbilityCategory defaultCategory,
    int defaultCharges = 1) {
    public string AbilityDefName { get; } = abilityDefName;
    public string PackageId { get; } = packageId;
    public BottledAbilityCategory DefaultCategory { get; } = defaultCategory;
    public int DefaultCharges { get; } = defaultCharges;

    public string JarDefName => $"VortexBA_{AbilityDefName}";
}