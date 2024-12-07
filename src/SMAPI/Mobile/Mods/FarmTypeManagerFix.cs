using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace StardewModdingAPI.Mobile.Mods;

internal static class FarmTypeManagerFix
{
    internal static void Apply()
    {
        SCoreMobileManager.OnAllModLoaded += OnAllModLoaded;
    }

    private static void OnAllModLoaded()
    {
        Console.WriteLine("start FarmTypeManagerFix Apply()");
        var hp = new Harmony("Esca.FarmTypeManager");
        var method = hp.GetPatchedMethods();
        foreach (var m in method)
        {
            if (m.Name == "isCollidingPosition")
            {
                hp.Unpatch(m, HarmonyPatchType.All);
                Console.WriteLine("Fixed isCollidingPosition Unpatch");
                break;
            }
        }
        Console.WriteLine("done FarmTypeManagerFix");
    }
}
