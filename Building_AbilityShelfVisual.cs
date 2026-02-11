using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace BottledAbilities;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public class Building_AbilityShelfVisual : Building_Storage {
    private const string StageA = "VortexBA_AbilityShelf_StageA";
    private const string StageB = "VortexBA_AbilityShelf_StageB";
    private const string StageC = "VortexBA_AbilityShelf_StageC";
    private const int StageAMaxCount = 0;
    private const int StageBMaxCount = 2;

    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        RefreshShelfStyle();
    }

    public override void Notify_ReceivedThing(Thing newItem) {
        base.Notify_ReceivedThing(newItem);
        RefreshShelfStyle();
    }

    public override void Notify_LostThing(Thing newItem) {
        base.Notify_LostThing(newItem);
        RefreshShelfStyle();
    }

    public override void DrawGUIOverlay() {
        if (!Spawned || Find.CameraDriver.CurrentZoom != CameraZoomRange.Closest) return;

        var totalCount = GetSlotGroup()?.HeldThings.Sum(static thing => thing.stackCount) ?? 0;
        if (totalCount <= 0) return;

        GenMapUI.DrawThingLabel(this, totalCount.ToStringCached());
    }

    private void RefreshShelfStyle() {
        var slotCount = GetSlotGroup()?.HeldThingsCount ?? 0;
        var styleDefName = slotCount switch {
            <= StageAMaxCount => StageA,
            <= StageBMaxCount => StageB,
            _ => StageC
        };
        var styleDef = DefDatabase<ThingStyleDef>.GetNamedSilentFail(styleDefName);

        StyleDef = styleDef;

        if (Spawned) {
            DirtyMapMesh(Map);
        }
    }
}