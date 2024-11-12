using System;
using System.IO;
using System.Reflection;
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

        //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

        AndroidLogger.Log("Done InitFormSMAPILoader()");
    }

    private static Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        //fix dll "Stardew Valley.dll" pc to "StardewValley.dll" android
        string assemblyName = new AssemblyName(args.Name).Name;
        Console.WriteLine("try resolve assembly: " + assemblyName);
        return null;
    }

    static Assembly LoadDll(string fileName)
    {
        string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        return Assembly.LoadFrom(Path.Combine(currentDir, fileName));
    }
}
