using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StardewModdingAPI.Mobile;

internal static class SCoreMobileManager
{
    static bool isOnLoadMods = false;
    static object _lock = new();
    public static bool IsOnLoadMods
    {
        get
        {
            lock (_lock)
            {
                return isOnLoadMods;
            }
        }
        set
        {
            lock (_lock)
            {
                isOnLoadMods = value;
            }
        }
    }
}
