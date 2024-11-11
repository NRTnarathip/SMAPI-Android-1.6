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
using Android.Views;
using Microsoft.Xna.Framework;

namespace StardewModdingAPI.Android;

[HarmonyPatch]
public static class MainActivityPatcher
{

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Game), "Run", [])]
    static bool PrefixGameRun(Game __instance)
    {
        Console.WriteLine("Prefix Game.Run(): " + __instance);
        Console.WriteLine("gamePtr: " + GameRunner.instance.gamePtr);
        var _game1Field = AccessTools.Field(typeof(MainActivity), "_game1");
        Console.WriteLine("main activity gamePtr: " + _game1Field.GetValue(MainActivity.instance));

        return true;
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Game), "Run", [])]
    static void PostfixGameRun(Game __instance)
    {
        Console.WriteLine("postfix game.Run()");
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainActivity), "OnCreatePartTwo")]
    static bool OnCreatePartTwoFix(StardewValley.MainActivity __instance)
    {
        AndroidLogger.Log("On OnCreatePartTwoFix()");
        try
        {

            var MobileDisplay = AccessTools.TypeByName("StardewValley.Mobile.MobileDisplay");
            MobileDisplay.GetMethod("SetupDisplaySettings", BindingFlags.Static | BindingFlags.Public).Invoke(null, null);

            var activity = __instance;
            activity.SetPaddingForMenus();

            //setup SMAPI & SGameRunner
            bool isRunSMAPI = true;
            if (isRunSMAPI)
            {
                Program.Main([]);
                var gameRunner = GameRunner.instance;
            }
            else
            {
                var gameRunner = new GameRunner();
                GameRunner.instance = gameRunner;
            }

            activity.SetContentView((View)GameRunner.instance.Services.GetService(typeof(View)));
            GameRunner.instance.Run();


            AndroidLogger.Log("Successfully RunGame()");
        }
        catch (Exception ex)
        {
            AndroidLogger.Log("Error try RunGame(): " + ex);
        }
        Console.WriteLine("End OnCreatePartTwoFix()");

        return false;
    }
}
