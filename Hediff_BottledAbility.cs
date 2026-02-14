using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace BottledAbilities;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public class Hediff_BottledAbility : HediffWithComps {
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

    public override string LabelBase => _abilityDef?.label ?? base.LabelBase;

    public override string Description => _abilityDef?.description ?? base.Description;

    public override string LabelInBrackets {
        get {
            var chargesLabel = $"{_charges}x";
            var baseLabel = base.LabelInBrackets;

            if (baseLabel.NullOrEmpty()) {
                return chargesLabel;
            }

            return $"{chargesLabel}, {baseLabel}";
        }
    }

    public override bool TryMergeWith(Hediff other) {
        if (other is not Hediff_BottledAbility otherBottled ||
            _abilityDef != otherBottled._abilityDef) return false;

        _charges += otherBottled._charges;
        return true;
    }

    public override void PostAdd(DamageInfo? dinfo) {
        base.PostAdd(dinfo);
        ApplyDurationSettings();
        if (_abilityDef is null || pawn.abilities is null) return;

        if (pawn.abilities.GetAbility(_abilityDef, includeTemporary: true) is null) {
            pawn.abilities.GainAbility(_abilityDef);
        }
    }

    public override void PostRemoved() {
        base.PostRemoved();
        if (_abilityDef is null || pawn.abilities is null) return;

        var ability = pawn.abilities.GetAbility(_abilityDef);
        if (ability is not null) {
            pawn.abilities.RemoveAbility(_abilityDef);
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Defs.Look(ref _abilityDef, "abilityDef");
        Scribe_Values.Look(ref _charges, "charges");
    }

    private void ApplyDurationSettings() {
        var durationComp = GetComp<HediffComp_DisappearsDisableable>();
        if (durationComp is null) return;

        var settings = BottledAbilitiesMod.Settings;
        settings.InitializeIfNeeded();
        durationComp.SetDuration(settings.TemporaryDurationTicks);
        durationComp.disabled = !settings.IsTemporaryDurationEnabled;
    }
}
