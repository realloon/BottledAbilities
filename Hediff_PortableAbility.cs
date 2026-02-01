using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace PortableAbility;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public class Hediff_PortableAbility : HediffWithComps {
    private AbilityDef? _abilityDef;
    private int _charges;

    public AbilityDef? AbilityDef {
        get => _abilityDef;
        set => _abilityDef = value;
    }

    public int Charges {
        get => _charges;
        set => _charges = value;
    }

    public override string LabelInBrackets => $"{_charges}x";

    public override bool TryMergeWith(Hediff other) {
        if (other is not Hediff_PortableAbility otherPortable ||
            _abilityDef != otherPortable._abilityDef) return false;

        _charges += otherPortable._charges;
        return true;
    }

    public override void PostAdd(DamageInfo? dinfo) {
        base.PostAdd(dinfo);
        if (_abilityDef == null || pawn.abilities == null) return;

        if (pawn.abilities.GetAbility(_abilityDef) == null) {
            pawn.abilities.GainAbility(_abilityDef);
        }
    }

    public override void PostRemoved() {
        base.PostRemoved();
        if (_abilityDef == null || pawn.abilities == null) return;

        var ability = pawn.abilities.GetAbility(_abilityDef);
        if (ability != null) {
            pawn.abilities.RemoveAbility(_abilityDef);
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Defs.Look(ref _abilityDef, "abilityDef");
        Scribe_Values.Look(ref _charges, "charges");
    }
}