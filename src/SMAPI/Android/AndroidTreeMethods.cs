using System.Reflection;
using HarmonyLib;
using StardewValley.TerrainFeatures;

namespace StardewModdingAPI.Android;

internal static class AndroidTreeMethods
{
    static MethodBase ClearCache_Method = AccessTools.Method(typeof(Tree), nameof(ClearCache));
    public static void ClearCache()
    {
        ClearCache_Method.Invoke(null, null);
    }
}
