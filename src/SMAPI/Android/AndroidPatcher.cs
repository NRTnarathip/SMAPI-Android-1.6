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
    internal static void InitFormSMAPILoader()
    {

        AndroidLogger.Log("===========================");
        AndroidLogger.Log("===========================");
        AndroidLogger.Log("On InitFormSMAPILoader()");


        Log.enabled = true;

        //Harmony.DEBUG = false;
        //harmony = new Harmony(nameof(AndroidPatcher));
        //harmony.PatchAll();

        AndroidLogger.Log("Done InitFormSMAPILoader()");
    }
}
