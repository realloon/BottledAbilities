using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace PortableAbility;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public class Hediff_PortableAbility : HediffWithComps {
    public AbilityDef? AbilityDef;
    public int Charges;

    public override string LabelInBrackets => $"{Charges}x";

    public override bool TryMergeWith(Hediff other) {
        if (other is not Hediff_PortableAbility otherPortable ||
            AbilityDef != otherPortable.AbilityDef) return false;

        Charges += otherPortable.Charges;
        return true;
    }
    
    public override void PostAdd(DamageInfo? dinfo) {
        base.PostAdd(dinfo);
        if (AbilityDef == null || pawn.abilities == null) return;

        if (pawn.abilities.GetAbility(AbilityDef) == null) {
            pawn.abilities.GainAbility(AbilityDef);
        }
    }

    public override void PostRemoved() {
        base.PostRemoved();
        if (AbilityDef == null || pawn.abilities == null) return;

        var ability = pawn.abilities.GetAbility(AbilityDef);
        if (ability != null) {
            pawn.abilities.RemoveAbility(AbilityDef);
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Defs.Look(ref AbilityDef, "abilityDef");
        Scribe_Values.Look(ref Charges, "charges");
    }
}