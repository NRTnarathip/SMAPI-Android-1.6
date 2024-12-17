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
using StardewModdingAPI.Mobile.Facade;
using StardewModdingAPI.Mobile.Mods;
using StardewValley;
using StardewValley.Pathfinding;
using StardewValley.SpecialOrders.Objectives;

namespace StardewModdingAPI.Mobile;

[HarmonyPatch]
internal static class AndroidPatcher
{
    public static Harmony harmony { get; private set; }
    internal static void Setup()
    {
        AndroidLogger.Log("===========================");
        AndroidLogger.Log("===========================");
        AndroidLogger.Log("On AndroidPatcher.Setup()");

        try
        {
            Log.enabled = true;

            harmony = new Harmony(nameof(AndroidPatcher));
            harmony.PatchAll();
            VersionInfoMenu.Init();
            var modFix = AndroidModFixManager.Init();

            FarmTypeManagerFix.Init(modFix);
            SpaceCoreFix.Init(modFix);
            StardewAudioMethods.Init(modFix);
        }
        catch (Exception ex)
        {
            AndroidLogger.Log(ex);
        }
    }
}
