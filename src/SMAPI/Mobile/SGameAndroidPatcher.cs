using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;

namespace StardewModdingAPI.Mobile;

[HarmonyPatch]
internal static class SGameAndroidPatcher
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Game1), "AfterLoadContent")]
    public static void Postfix_AfterLoadContent()
    {
        IsAfterLoadContent = true;
        OnAfterLoadContent.Invoke();
    }
    public static bool IsAfterLoadContent = false;
    public static Action OnAfterLoadContent;
}
