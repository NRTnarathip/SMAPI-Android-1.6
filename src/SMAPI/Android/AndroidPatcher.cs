using System;
using HarmonyLib;
using StardewValley;

namespace StardewModdingAPI.Android;

[HarmonyPatch]
internal static class AndroidPatcher
{
    public static Harmony harmony { get; private set; }
    static void PrefixCheckAppPermissions()
    {
        AndroidLogger.Log("Prefix CheckAppPermissions()");
    }
    static void TestHook()
    {
        AndroidLogger.Log("Hello World");
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AndroidPatcher), nameof(TestHook))]
    static void PrefixTestHook()
    {
        AndroidLogger.Log("Prefix Test Hook");
    }
    internal static void InitFormSMAPILoader()
    {
        AndroidLogger.Log("===========================");
        AndroidLogger.Log("===========================");
        AndroidLogger.Log("On InitFormSMAPILoader()");


        Harmony.DEBUG = false;
        harmony = new Harmony(nameof(AndroidPatcher));
        harmony.PatchAll();

        var CheckAppPermissions = AccessTools.Method(typeof(MainActivity), nameof(MainActivity.CheckAppPermissions));
        AndroidLogger.Log("CheckAppPermissions: " + CheckAppPermissions);
        var detourMethod = AccessTools.Method(typeof(AndroidPatcher), nameof(PrefixCheckAppPermissions));
        AndroidLogger.Log("detour method: " + detourMethod);

        try
        {
            var patched = harmony.Patch(CheckAppPermissions, new(detourMethod));
            AndroidLogger.Log("after patch: " + patched);
            //TestHook();
        }
        catch (Exception ex)
        {
            AndroidLogger.Log("harmony err: " + ex);
        }


        AndroidLogger.Log("Done InitFormSMAPILoader()");
    }
}
