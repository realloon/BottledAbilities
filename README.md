# Bottled Abilities

**Bottled Abilities** lets abilities be bottled.

## Project Structure

- `CompProperties/`
- `Helpers/`
- `HarmonyPatches/`
- `ModConfigs/`
- `Types/`: Data type definitions
- `BottledAbilities.cs`: Entry point
- `Hediff_BottledAbility.cs`: Ability manager
- `BottledAbilityCatalog.cs` + `DefGenerator.cs`: Dynamic mapping and generation
- `CompEmptyAbilityJar.cs`
- `JobDriver_InfuseAbilityIntoJar.cs`
- `IngestionOutcomeDoer_GiveBottledAbility.cs`

## Development

- **Dependency**: Dependencies are managed via NuGet (`Krafs.Rimworld.Ref`, `Lib.Harmony.Ref`).
- **Build**: Use `dotnet build`, and make sure the output path points to your local RimWorld mod `Assemblies` directory.
