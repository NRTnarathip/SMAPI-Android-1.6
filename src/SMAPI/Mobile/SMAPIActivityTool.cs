using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewModdingAPI.Framework;
using StardewValley;

namespace StardewModdingAPI.Mobile;

internal static class SMAPIActivityTool
{
    public static void ExitGame()
    {
        IMonitor? monitor = SCore.Instance?.GetMonitorForGame();
        monitor?.Log("Try Exit Game At SMAPIActivityTool");

        var activityField = AccessTools.Field(typeof(MainActivity), nameof(MainActivity.instance));
        var activity = activityField.GetValue(null) as Android.App.Activity;
        activity.Finish();

        monitor?.Log("Done Exit Game.");
    }
}
