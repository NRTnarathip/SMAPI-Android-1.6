using HarmonyLib;

namespace StardewModdingAPI.Android;

internal static class AndroidPatcher
{
    public static Harmony harmony { get; private set; }
    public static void BeforeProgramMain()
    {
        harmony = new Harmony(nameof(AndroidPatcher));
        harmony.PatchAll();
        AndroidLogger.Log("game data path: " + Constants.DataPath);
    }
}
