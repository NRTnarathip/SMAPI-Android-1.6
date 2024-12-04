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
    static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    public static bool IsOnLoadMods
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return isOnLoadMods;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        set
        {
            _lock.EnterWriteLock();
            try
            {
                isOnLoadMods = value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
