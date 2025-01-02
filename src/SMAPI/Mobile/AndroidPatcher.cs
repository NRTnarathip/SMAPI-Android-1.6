using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Force.DeepCloner;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Internal;
using StardewModdingAPI.Mobile.Facade;
using StardewModdingAPI.Mobile.Mods;
using StardewModdingAPI.Mobile.Vectors;
using StardewValley;
using StardewValley.Pathfinding;
using StardewValley.SpecialOrders.Objectives;

namespace StardewModdingAPI.Mobile;

[HarmonyPatch]
internal static class AndroidPatcher
{
    public static Harmony? harmony { get; private set; }
    internal static void Setup()
    {
        AndroidLogger.Log("===========================");
        AndroidLogger.Log("===========================");
        AndroidLogger.Log("On AndroidPatcher.Setup()");

        try
        {
            //setup
            Log.enabled = true;
            harmony = new Harmony(nameof(AndroidPatcher));

            VersionInfoMenu.Init();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error on AndroidPatcher.Setup()");
            AndroidLogger.Log(ex);
            throw;
        }
    }
    static void ApplyHarmonyPatchAll()
    {
        var monitor = SCore.Instance.SMAPIMonitor;
        monitor.Log("On ApplyHarmonyPatchAll()..");
        try
        {
            harmony.PatchAll();
            monitor.Log("Done harmony.PatchAll()");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            monitor.Log(ex.GetLogSummary(), LogLevel.Error);
            throw;
        }
    }
    static void SetupModFix()
    {
        //Register mod fix here
        var modFix = AndroidModFixManager.Init();
        //list mods
        FarmTypeManagerFix.Init(modFix);
        SpaceCoreFix.Init(modFix);
        SveFix.Init(modFix);
        GenericConfigMenuModFix.Init(modFix);
        UnlockableBundlesModFix.Init(modFix);
    }

    internal static void OnBeforeSCoreRun()
    {
        SetupModFix();
        ApplyHarmonyPatchAll();
        VectorTypeConverterFix.ApplyPatch(harmony);
    }
}
