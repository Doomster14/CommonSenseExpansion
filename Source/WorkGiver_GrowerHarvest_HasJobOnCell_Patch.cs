using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CommonSenseExpansion
{
    [HarmonyPatch(typeof(WorkGiver_GrowerHarvest)), HarmonyPatch(nameof(WorkGiver_GrowerHarvest.HasJobOnCell))]
    internal static class WorkGiver_GrowerHarvest_HasJobOnCell_Patch
    {
        // Implementing feature:
        // • The "Allow cutting" toggle in growing zones only affects sowing.
        //   This complements the process of switching to a new plant in the zone.
        //   In the original game, only the designated plant can be harvested when cutting is disabled.
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            //Log.Message($"Transpiler: Got {codes.Count} instructions.");

            // Solution #1.
            // Trying to remove the following vanilla code:
            //   if (c.GetZone(pawn.Map) is Zone_Growing zone_Growing && !zone_Growing.allowCut && plant.def != WorkGiver_Grower.wantedPlantDef)
            //   {
            //       return false;
            //   }
            // This solution is clean and efficient, no unused code is left behind.
            // On the other hand, more chances to break transpiles of other mods, compared to solution #2, see below.
            // Looking for pattern, starting from the end:
            //   labels: ldarg.2
            //   [anything but ret]...
            //   ldfld bool RimWorld.Zone_Growing::allowCut
            //   [anything but ret]...
            //   ret
            //   labels: ...
            // Removing the pattern from the start to the "ret".
            // The last label set is supposed to only be used inside the pattern and can be removed.
            // But if transpile fails because of missing labels, then not only this patch fails, other patches will stop functioning too.
            // So, adding the first label set to the last one.
            for (int iLdarg = codes.Count - 1; iLdarg >= 0; --iLdarg)
            {
                if (codes[iLdarg].opcode == OpCodes.Ldarg_2 && codes[iLdarg].labels.Count != 0)
                {
                    //Log.Message($"Transpiler: Found {codes[iLdarg].labels.Count} labels for ldarg.2 at offset {iLdarg}.");
                    for (int iAllowCut = iLdarg + 1; iAllowCut < codes.Count; ++iAllowCut)
                    {
                        if (codes[iAllowCut].opcode == OpCodes.Ret)
                            break;
                        if (codes[iAllowCut].opcode == OpCodes.Ldfld &&
                            codes[iAllowCut].LoadsField(AccessTools.Field(typeof(Zone_Growing), nameof(Zone_Growing.allowCut))))
                        {
                            for (int iRet = iAllowCut + 1; iRet < codes.Count - 1; ++iRet)
                            {
                                if (codes[iRet].opcode == OpCodes.Ret)
                                {
                                    if (codes[iRet + 1].labels.Count == 0)
                                        break;

                                    //Log.Message($"Transpiler: Found {codes[iRet + 1].labels.Count} labels after ret at offset {iRet + 1}.");
                                    codes[iRet + 1].labels.AddRange(codes[iLdarg].labels);
                                    codes.RemoveRange(iLdarg, iRet + 1 - iLdarg);
                                    //Log.Message($"Transpiler: Removed {iRet + 1 - iLdarg} instructions.");
                                    return codes.AsEnumerable();
                                }
                            }
                        }
                    }
                }
            }

            // Solution #2.
            // This is a backup solution for when solution #1 doesn't work.
            // While solution #1 has some flexibility and may work when the vanilla code changes
            // or the method is transpiled by another mod, it may not.
            // Trying a dirty, not effecient, but much more robust solution: replacing all reads of Zone_Growing.allowCut field with true.
            // Looking for all instructions, starting from the end:
            //   ldfld Zone_Growing.allowCut
            // Replacing with:
            //   pop
            //   ldc.i4.1
            bool found = false;
            for (int i = codes.Count - 1; i >= 0; --i)
            {
                if (codes[i].opcode == OpCodes.Ldfld &&
                    codes[i].LoadsField(AccessTools.Field(typeof(Zone_Growing), nameof(Zone_Growing.allowCut))))
                {
                    found = true;
                    codes[i].opcode = OpCodes.Pop;
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldc_I4_1));
                }
            }

            if (!found)
            {
                Log.Error($"[{TheMod.name}] Error: Failed to transpile"
                    + $" {nameof(WorkGiver_GrowerHarvest)}.{nameof(WorkGiver_GrowerHarvest.HasJobOnCell)}()."
                    + " Feature not applied: The \"Allow cutting\" toggle in growing zones only affects sowing.");
            }
#if DEBUG
            else
            {
                // The warning is only supposed to be seen in Debug configuration, by developers.
                Log.Warning($"[{TheMod.name}] Warning: Failed to transpile"
                    + $" {nameof(WorkGiver_GrowerHarvest)}.{nameof(WorkGiver_GrowerHarvest.HasJobOnCell)}() with solution #1."
                    + " Solution #2 applied successfully for feature: The \"Allow cutting\" toggle in growing zones only affects sowing.");
            }
#endif

            return codes.AsEnumerable();
        }
    }
}
