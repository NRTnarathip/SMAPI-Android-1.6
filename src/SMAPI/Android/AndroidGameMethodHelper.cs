using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;

namespace StardewModdingAPI.Android;

internal static class AndroidGameMethodHelper
{
    static MethodInfo UpdateTitleScreenDuringLoadingMode_Method;
    static AndroidGameMethodHelper()
    {
        UpdateTitleScreenDuringLoadingMode_Method = AccessTools.Method(typeof(Game1), "UpdateTitleScreenDuringLoadingMode");
    }
    public static void UpdateTitleScreenDuringLoadingMode()
    {
        UpdateTitleScreenDuringLoadingMode_Method.Invoke(Game1.game1, null);
    }
}
