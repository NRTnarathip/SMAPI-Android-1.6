using System;
using HarmonyLib;
using StardewValley;

namespace StardewModdingAPI.Android;

[HarmonyPatch]
internal static class AndroidPatcher
{
    public static Harmony harmony { get; private set; }
    //static void Test()
    //{
    //    AndroidLogger.Log("On Test()");
    //}
    //[HarmonyPrefix]
    //[HarmonyPatch(typeof(AndroidPatcher), nameof(AndroidPatcher.Test))]
    //static void PrefixTest()
    //{
    //    AndroidLogger.Log("On PrefixTest()");
    //}
    internal static void InitFormSMAPILoader()
    {

        AndroidLogger.Log("===========================");
        AndroidLogger.Log("===========================");
        AndroidLogger.Log("On InitFormSMAPILoader()");

        Log.enabled = true;

        Harmony.DEBUG = false;
        harmony = new Harmony(nameof(AndroidPatcher));
        harmony.PatchAll();

        AndroidLogger.Log("Done InitFormSMAPILoader()");
    }
}
