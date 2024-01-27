using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CommonSenseExpansion
{
    [HarmonyPatch(typeof(Need_Food)), HarmonyPatch(nameof(Need_Food.FoodFallPerTickAssumingCategory))]
    internal static class Need_Food_FoodFallPerTickAssumingCategory_Patch
    {
        // Implementing feature:
        // • Fix bug: Malnutrition ailment doesn't affect hunger rate.
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            // Trying to replace the vanilla code snippet
            //   ignoreMalnutrition ? null : HediffDefOf.Malnutrition
            // with
            //   ignoreMalnutrition ? HediffDefOf.Malnutrition : null
            // Looking for instructions:
            //   brtrue.s <Label>
            //   ldsfld class Verse.HediffDef RimWorld.HediffDefOf::Malnutrition
            // Replacing the first instruction with:
            //   brfalse.s <Label>
            for (int i = 0; i < codes.Count - 1; ++i)
            {
                if (codes[i].opcode == OpCodes.Brtrue_S &&
                    codes[i + 1].opcode == OpCodes.Ldsfld &&
                    codes[i + 1].LoadsField(AccessTools.Field(typeof(HediffDefOf), nameof(HediffDefOf.Malnutrition))))
                {
                    //Log.Message($"Transpiler: Found brtrue.s at offset {i}.");
                    codes[i].opcode = OpCodes.Brfalse_S;
                    return codes.AsEnumerable();
                }
            }

            Log.Error($"[{TheMod.name}] Error: Failed to transpile"
                + $" {nameof(Need_Food)}.{nameof(Need_Food.FoodFallPerTickAssumingCategory)}()."
                + " Feature not applied: Fix bug: Malnutrition ailment doesn't affect hunger rate.");

            return codes.AsEnumerable();
        }
    }
}
