using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Android.Views;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Framework;
using StardewValley;

namespace StardewModdingAPI.Mobile;

[HarmonyPatch]
internal static class AppNotRespondFix
{
    public static void Init()
    {
        var harmony = new Harmony(nameof(AppNotRespondFix));
    }

}
