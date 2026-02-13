using JetBrains.Annotations;
using UnityEngine;
using Verse;

// ReSharper disable InconsistentNaming

namespace BottledAbilities;

public sealed class BottledAbilityCategoryColorEntry : IExposable {
    public AbilityCategory category;
    public Color color = Color.white;

    [UsedImplicitly]
    public BottledAbilityCategoryColorEntry() { }

    public BottledAbilityCategoryColorEntry(AbilityCategory category, Color color) {
        this.category = category;
        this.color = color;
    }

    public void ExposeData() {
        Scribe_Values.Look(ref category, "category", AbilityCategory.Utility);
        Scribe_Values.Look(ref color, "color", Color.white);
    }
}