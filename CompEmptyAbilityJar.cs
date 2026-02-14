using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace BottledAbilities;

[UsedImplicitly]
public class CompEmptyAbilityJar : ThingComp {
    private const string InfuseJobDefName = "VortexBA_InfuseAbilityIntoJar";
    private const int InfuseStackCount = 1;

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn) {
        if (selPawn is null) yield break;
        if (!selPawn.IsColonistPlayerControlled) yield break;
        if (parent is null || !parent.Spawned) yield break;
        if (selPawn.MapHeld != parent.MapHeld) yield break;

        var openLabel = "VortexBA_FloatMenuInfuseOpen".Translate();

        // Reserve only 1 from the stack. Reserving the entire stack can fail if other pawns
        // have reserved some of it (even though one unit is still available).
        if (!selPawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly, 1, InfuseStackCount)) {
            yield return new FloatMenuOption(openLabel, null);
            yield break;
        }

        yield return new FloatMenuOption(openLabel, delegate {
            OpenInfuseMenu(selPawn);
        });
    }

    private void OpenInfuseMenu(Pawn pawn) {
        var options = new List<FloatMenuOption>();

        var settings = BottledAbilitiesMod.Settings;
        settings.InitializeIfNeeded();

        foreach (var ability in pawn.abilities?.AllAbilitiesForReading ?? Enumerable.Empty<Ability>()) {
            var def = ability.def;
            if (def is null) continue;
            var abilityLabel = def.label.NullOrEmpty() ? GenText.SplitCamelCase(def.defName) : def.label;

            // Must be enabled in mod settings.
            if (!settings.IsEnabled(def.defName)) continue;

            // Exclude abilities that are temporarily granted via bottled abilities (Hediff_BottledAbility).
            if (pawn.health?.hediffSet?.hediffs?.OfType<Hediff_BottledAbility>()
                    .Any(h => h.AbilityDef == def) == true) {
                continue;
            }

            var jarDefName = $"VortexBA_{def.defName}";
            var jarDef = DefDatabase<ThingDef>.GetNamedSilentFail(jarDefName);
            if (jarDef is null) continue;

            var charges = settings.GetCharges(def.defName);
            var baseLabel = (string)"VortexBA_FloatMenuInfuseOption".Translate(abilityLabel, charges);

            var disabled = ability.GizmoDisabled(out var reason);
            var label = disabled && !reason.NullOrEmpty()
                ? $"{baseLabel}: {reason}"
                : baseLabel;

            options.Add(new FloatMenuOption(label, disabled ? null : delegate {
                StartInfuseJob(pawn, ability);
            }));
        }

        if (options.Count == 0) {
            options.Add(new FloatMenuOption("VortexBA_FloatMenuNoEligibleAbilities".Translate(), null));
        }

        Find.WindowStack.Add(new FloatMenu(options));
    }

    private void StartInfuseJob(Pawn pawn, Ability ability) {
        if (!pawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly, 1, InfuseStackCount)) {
            Messages.Message("VortexBA_MessageInfuseFailed"
                .Translate("VortexBA_CannotReserveOrReach".Translate()), pawn, MessageTypeDefOf.RejectInput);
            return;
        }

        var jobDef = DefDatabase<JobDef>.GetNamedSilentFail(InfuseJobDefName);
        if (jobDef is null) {
            Log.ErrorOnce($"[BottledAbilities] Missing JobDef '{InfuseJobDefName}'.", 88612173);
            Messages.Message("VortexBA_MessageInfuseFailed".Translate("VortexBA_MissingJobDef".Translate(InfuseJobDefName)),
                pawn, MessageTypeDefOf.RejectInput);
            return;
        }

        var job = JobMaker.MakeJob(jobDef, parent);
        job.ability = ability;
        job.count = InfuseStackCount;
        if (pawn.jobs is null) {
            Messages.Message("VortexBA_MessageInfuseFailed"
                .Translate("VortexBA_CannotStartJob".Translate()), pawn, MessageTypeDefOf.RejectInput);
            return;
        }

        if (!pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc)) {
            Messages.Message("VortexBA_MessageInfuseFailed"
                .Translate("VortexBA_CannotStartJob".Translate()), pawn, MessageTypeDefOf.RejectInput);
        }
    }
}
