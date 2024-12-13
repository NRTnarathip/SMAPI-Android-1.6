using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StardewModdingAPI.Mobile;

internal static class SCoreMobileManager
{
    public enum LoadModsStateEnum
    {
        None = 0,
        Starting = 1,
        LoadedAndNeedToConfirm = 2,
        LoadedConfirm = 3,
    }
    static object _lock = new();
    static LoadModsStateEnum _loadModsState = LoadModsStateEnum.None;
    public static LoadModsStateEnum LoadModsState
    {
        get
        {
            lock (_lock)
            {
                return _loadModsState;
            }
        }
        set
        {
            lock (_lock)
            {
                _loadModsState = value;
            }
        }
    }

    public static int WaitFirstTickForContentLoaded { get; internal set; }
}
