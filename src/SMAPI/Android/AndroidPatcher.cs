using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
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


        Log.enabled = true;

        Harmony.DEBUG = false;
        harmony = new Harmony(nameof(AndroidPatcher));
        harmony.PatchAll();

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
