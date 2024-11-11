using System;

namespace StardewModdingAPI.Android;

static class AndroidConsolePatcher
{
    //[HarmonyPrefix]
    //[HarmonyPatch(typeof(Console), "set_Title")]
    //static bool PrefixSetTitle(ref string value)
    //{
    //    AndroidLogger.Log("bypass Console.set_Title");
    //    return false;
    //}
    //[HarmonyPrefix]
    //[HarmonyPatch(typeof(Console), nameof(Console.WriteLine), [typeof(string)])]
    //public static void WriteLine(string value)
    //{
    //    AndroidLogger.Log("Console.WriteLine(string)= " + value);
    //}
}
