using System;
using System.IO;
using System.Reflection;
using Force.DeepCloner;
using HarmonyLib;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI.Mobile.Mods;
using StardewValley;

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
        }
        catch (Exception ex)
        {
            AndroidLogger.Log(ex);
        }
        AndroidLogger.Log("Done Setup()");
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Game1), "TranslateFields")]
    static void PrefixTranslateFields()
    {
        Console.WriteLine("prefix TranslateFields");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Game1), "TranslateFields")]
    static void PostfixTranslateFields()
    {
        Console.WriteLine("post TranslateFields");
    }
}
