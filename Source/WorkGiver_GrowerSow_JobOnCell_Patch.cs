using HarmonyLib;
using RimWorld;
using Verse;

namespace CommonSenseExpansion
{
    [HarmonyPatch(typeof(WorkGiver_GrowerSow)), HarmonyPatch(nameof(WorkGiver_GrowerSow.JobOnCell))]
    internal static class WorkGiver_GrowerSow_JobOnCell_Patch
    {
        // Implementing feature:
        // • Sown plants are not cut down when a growing zone is switched to a different plant.
        private static bool Prefix(ref Verse.AI.Job __result, ref ThingDef ___wantedPlantDef, Pawn pawn, IntVec3 c)
        {
            Map map = pawn.Map;
            Plant plant = c.GetPlant(map);
            // Is the cell occupied by a sown plant?
            if (plant != null && plant.sown)
            {
                // Result of the original method. Null stands for 'job is not available for the cell'.
                // Unlike the original method, don't cut the sown plant.
                __result = null;
                // Tell Harmony to skip the original method.
                return false;
            }

            if (___wantedPlantDef == null)
            {
                // Minor optimization: we change the static property, avoiding re-calculation by the original method.
                ___wantedPlantDef = WorkGiver_Grower.CalculateWantedPlantDef(c, map);
            }
            if (___wantedPlantDef != null)
            {
                // The original method only tries to deal with the first found blocker. So do we.
                Thing blocker = PlantUtility.AdjacentSowBlocker(___wantedPlantDef, c, map);
                // Is sowing blocked by a sown plant?
                if (blocker != null && blocker is Plant blockerPlant && blockerPlant.sown)
                {
                    // Result of the original method. Null stands for 'job is not available for the cell'.
                    // Unlike the original method, don't cut the sown plant.
                    __result = null;
                    // Tell Harmony to skip the original method.
                    return false;
                }
            }

            // Tell Harmony to go on with the original method.
            return true;
        }
    }
}
