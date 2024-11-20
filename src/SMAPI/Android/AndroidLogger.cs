using AndroidUtils = Android.Util;

namespace StardewModdingAPI.Mobile;

public static class AndroidLogger
{
    const string Tag = "SMAPI-Tag";
    public static void Log(object msg) => AndroidUtils.Log.Debug(Tag, msg.ToString());
}
