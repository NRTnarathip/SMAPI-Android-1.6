using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using StardewModdingAPI.Framework;
using StardewValley.Menus;

namespace StardewModdingAPI.Mobile;

static class ModQuickSaveOptionPage
{
    static string QuickSaveButtonText = "Quick Save";
    static string LoadQuickSaveButtonText = "Load Last Quick Save";
    static bool m_isQuickSaveModAvailable = false;
    static Assembly modAssembly;
    static Type MainType;
    static MethodInfo TrySaveMethod;
    static MethodInfo TryLoadMethod;
    static IMonitor Monitor;
    internal static void Init(AndroidModFixManager modFix)
    {
        Monitor = SCore.Instance.GetMonitorForGame();
        modFix.RegisterOnModLoaded("QuickSave.dll", (asm) =>
        {
            m_isQuickSaveModAvailable = true;
            modAssembly = asm;
            MainType = modAssembly.GetType("QuickSave.Lib.Main");
            TrySaveMethod = AccessTools.Method(MainType, "TrySave");
            TryLoadMethod = AccessTools.Method(MainType, "TryLoad");
        });
    }

    internal static void SetupOptionPage(OptionsPage page, ref List<OptionsElement> options)
    {
        if (!m_isQuickSaveModAvailable)
            return;

        var btnQuickSave = new OptionsButton(QuickSaveButtonText, OnClickQuickSave);
        var btnLoadQuickSave = new OptionsButton(LoadQuickSaveButtonText, OnClickLoadQuickSave);
        options.Insert(2, btnQuickSave);
        options.Insert(3, btnLoadQuickSave);
    }

    static void OnClickQuickSave()
    {
        try
        {
            TrySaveMethod.Invoke(null, null);
        }
        catch (Exception ex)
        {
            Monitor.Log($"Error invoking QuickSave TrySave(): {ex}", LogLevel.Error);
        }
    }
    static void OnClickLoadQuickSave()
    {
        try
        {
            TryLoadMethod.Invoke(null, null);
        }
        catch (Exception ex)
        {
            Monitor.Log($"Error invoking QuickSave TryLoad(): {ex}", LogLevel.Error);
        }
    }

}
