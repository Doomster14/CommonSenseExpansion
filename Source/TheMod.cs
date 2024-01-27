using HarmonyLib;
using Verse;

//**********************************************************//
// For publishing, should be built in Release configuration //
//**********************************************************//

// To do:
// Test compatibility with Smart Farming mod.
// Change the description of "Allow cutting" button in growing zones. When sowing, allow cutting of any blocking plants except previously sown ones. If a blocking plant is in a nearby growing zone, it will only be cut if cutting is allowed there too.
// Settings.
// Fix bug with double-blocker:
//   in some cases, a cell appears as sowable, but after a pawn cuts a blocker he/she still cannot sow because of another blocker.
// Fix bug: sowing seems enabled when skill is not enough for sowing but there is a blocker.
// Fix bug: forcing harvest may cancel a partially done work of a nearby harvesting pawn.
// Fix glitch: area harvest only applies to the chosen plant, other plants are harvested one-by-one. This complements the process of switching to a new plant in the zone.
// Fix bug: Malnutrition ailment severity rate is miscalculated. In RimWorld.Need_Food, BaseMalnutritionSeverityPerInterval is calculated as (BaseMalnutritionSeverityPerDay / RimWorld.NeedTunings.NeedUpdateInterval) while it should be (BaseMalnutritionSeverityPerDay / RimWorld.GenDate.TicksPerDay * RimWorld.NeedTunings.NeedUpdateInterval).

namespace CommonSenseExpansion
{
	internal class TheMod : Mod
	{
		public static string name;

		public TheMod(ModContentPack content) : base(content)
		{
			name = content.Name;

			Harmony harmony = new Harmony(id: content.PackageIdPlayerFacing);
			harmony.PatchAll();
			Log.Message($"[{name}] Initialized.");
		}
	}
}
