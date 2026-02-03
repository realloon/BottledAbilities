using JetBrains.Annotations;
using HarmonyLib;
using Verse;

// ReSharper disable InconsistentNaming

namespace PortableAbility.HarmonyPatches;

[UsedImplicitly]
[HarmonyPatch(typeof(Pawn_InventoryTracker), "GetGizmos")]
public class Postfix_Pawn_InventoryTracker_GetGizmos {
    [UsedImplicitly]
    [HarmonyPostfix]
    private static IEnumerable<Gizmo> AddPortableAbilityGizmos(IEnumerable<Gizmo> __result, Pawn ___pawn) {
        foreach (var gizmo in __result) {
            yield return gizmo;
        }

        if (!___pawn.IsColonistPlayerControlled || !___pawn.Drafted) yield break;

        foreach (var item in ___pawn.inventory.innerContainer) {
            var comp = item.TryGetComp<CompPortableAbility>();
            var gizmo = comp?.GetInventoryGizmoExtra(___pawn);

            if (gizmo != null) {
                yield return gizmo;
            }
        }
    }
}