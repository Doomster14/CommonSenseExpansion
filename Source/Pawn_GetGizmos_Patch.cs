#if DEBUG
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace CommonSenseExpansion
{
    [HarmonyPatch(typeof(Pawn)), HarmonyPatch(nameof(Pawn.GetGizmos))]
    internal static class Pawn_GetGizmos_Patch
    {
        private static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            __result = __result.Concat<Gizmo>(new Command_DebugInfo(__instance));
        }
    }

    public class Command_DebugInfo : Command_Action
    {
        private readonly Pawn pawn;

        public Command_DebugInfo(Pawn pawn)
        {
            this.pawn = pawn;
            defaultLabel = "Debug info";
            defaultDesc = "Show some debug info about the pawn in the debug log.";
            action = ShowDebugInfo;
        }

        private void ShowDebugInfo()
        {
            float MalSevPerInterval = (float)typeof(Need_Food).GetProperty("MalnutritionSeverityPerInterval",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(pawn.needs.food);
            Log.Message(
                $"Pawn: {pawn.ThingID} ({pawn.Name})"
                + $", Tick: {Find.TickManager.TicksGame}"
                + $", Saturation: {pawn.needs.food.CurLevel}/{pawn.needs.food.MaxLevel}"
                //+ $", Base fall: {Need_Food.BaseHungerRate(pawn.ageTracker.CurLifeStage, pawn.def) * GenDate.TicksPerDay}"
                + $", Fall: {pawn.needs.food.FoodFallPerTick * GenDate.TicksPerDay}"
                + $", Fixed: {pawn.needs.food.FoodFallPerTickAssumingCategory(pawn.needs.food.CurCategory, true) * GenDate.TicksPerDay}"
                + $", Malnutrition: {pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition)?.Severity * 100 ?? 0f}%"
                + $", Mal sev per 150: {MalSevPerInterval * 100}%"
                + $", Mal sev per hour: {MalSevPerInterval / NeedTunings.NeedUpdateInterval * GenDate.TicksPerHour * 100}%"
                //+ $", Hungry: {pawn.needs.food.PercentageThreshHungry * 100}%"
                //+ $", Ravenously Hungry: {pawn.needs.food.PercentageThreshUrgentlyHungry * 100}%"
                );
        }
    }
}
#endif
