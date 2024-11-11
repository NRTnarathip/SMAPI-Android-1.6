using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndroidUtils = Android.Util;
using System.Reflection;
using StardewValley;
using StardewModdingAPI.Framework;
using HarmonyLib;

namespace StardewModdingAPI.Android;

[HarmonyPatch]
public static class MainActivityPatcher
{
    public static void PrefixCheckAppPermissions()
    {
        AndroidLogger.Log("On Prefix CheckAppPermissions");
        StardewModdingAPI.Program.Main([]);
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainActivity), "OnCreatePartTwo")]
    static bool OnCreatePartTwoFix(StardewValley.MainActivity __instance)
    {
        AndroidLogger.Log("Prefix OnCreatePartTwoFix()");
        //Original Src Code
        //Log.It("MainActivity.OnCreatePartTwo");
        //MobileDisplay.SetupDisplaySettings();
        //SetPaddingForMenus();
        //GameRunner gameRunner = new GameRunner();
        //SetContentView((View)gameRunner.Services.GetService(typeof(View)));
        //GameRunner.instance = gameRunner;
        //_game1 = gameRunner.gamePtr;
        //gameRunner.Run();

        try
        {
            var mainActivity = __instance;
            var stardewAssembly = typeof(StardewValley.MainActivity).Assembly;
            var MobileDisplayType = stardewAssembly.GetType("StardewValley.Mobile.MobileDisplay");
            MobileDisplayType.GetMethod("SetupDisplaySettings", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
            mainActivity.SetPaddingForMenus();
            typeof(StardewValley.MainActivity)
                .GetField("_game1", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(mainActivity, SGameRunner.instance);
            SGameRunner.instance.Run();
            AndroidLogger.Log("Successful OnCreatePartTwoFix()");
        }
        catch (Exception ex)
        {
            AndroidLogger.Log("Error try OnCreatePartTwoFix()\n" + ex);
        }

        return false;
    }
    internal static void OnTrySGameRuner_Run()
    {
        AndroidLogger.Log("On OnTrySGameRuner_Run()");
    }
}
