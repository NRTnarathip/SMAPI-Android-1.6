namespace StardewModdingAPI.Mobile;

internal static class AndroidModLoaderManager
{
    public enum LoadStatus
    {
        None = 0,
        Starting = 1,
        LoadedAndNeedToConfirm = 2,
        LoadedConfirm = 3,
    }
    static object _lock = new();
    static LoadStatus _loadStatus = LoadStatus.None;
    public static LoadStatus CurrentStatus
    {
        get
        {
            lock (_lock)
            {
                return _loadStatus;
            }
        }
        set
        {
            lock (_lock)
            {
                _loadStatus = value;
            }
        }
    }
}
