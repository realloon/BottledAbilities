using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace BottledAbilities;

[UsedImplicitly]
public class JobDriver_InfuseAbilityIntoJar : JobDriver {
    private const int InfuseTicks = 240;
    private const int ReserveStackCount = 1;

    private Thing EmptyJar => job.GetTarget(TargetIndex.A).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        // Only reserve one jar from the stack.
        return pawn.Reserve(EmptyJar, job, 1, ReserveStackCount, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOn(() => job.ability is null);

        yield return Toils_Reserve.Reserve(TargetIndex.A, 1, ReserveStackCount);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

        yield return Toils_General.Wait(InfuseTicks)
            .WithProgressBarToilDelay(TargetIndex.A)
            .FailOnDespawnedOrNull(TargetIndex.A)
            // Must be touching the jar while performing the infusion (but not before walking there).
            .FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);

        yield return new Toil {
            initAction = delegate {
                var jar = EmptyJar;
                if (!AbilityInfusionUtility.TryInfuse(pawn, job.ability!, jar)) {
                    EndJobWith(JobCondition.Incompletable);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}
