using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;

namespace StardewModdingAPI.Mobile.Mods;

internal static class SveFix
{
    public static void Init(AndroidModFixManager modFix)
    {
        modFix.RegisterOnModLoaded("StardewValleyExpanded.dll", OnModLoaded);
    }
    static Type TMXLLoadMapFacingDirection;
    static void OnModLoaded(Assembly asm)
    {
        Console.WriteLine("Start SveFix");
        var harmony = AndroidPatcher.harmony;
        TMXLLoadMapFacingDirection = asm.GetType("StardewValleyExpanded.HarmonyPatch_TMXLLoadMapFacingDirection");
        harmony.Patch(
            original: AccessTools.Method(TMXLLoadMapFacingDirection, "ApplyPatch"),
            prefix: new(typeof(SveFix), nameof(TMXLLoadMapFacingDirection_ApplyPatch))
        );
        Console.WriteLine("Done SveFix");
    }
    static bool TMXLLoadMapFacingDirection_ApplyPatch(Harmony harmony, IMonitor monitor)
    {
        monitor.Log("Try fix TMXLLoadMapFacingDirection_ApplyPatch", LogLevel.Trace);
        monitor.Log("Applying Harmony patch \"HarmonyPatch_TMXLLoadMapFacingDirection\": " +
            "prefixing SDV method \"Game1.warpFarmer(LocationRequest, int, int, int, bool)\".");

        harmony.Patch(
            original: AccessTools.Method(typeof(Game1), "warpFarmer",
            [
                typeof(LocationRequest), typeof(int),
                typeof(int), typeof(int), typeof(bool),
            ]),
            prefix: new(TMXLLoadMapFacingDirection, "Game1_warpFarmer")
        );

        return false;
    }
}
