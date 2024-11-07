using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndroidUtils = Android.Util;

namespace StardewModdingAPI.Android;

public static class AndroidLogger
{
    const string Tag = "SMAPI-Tag";
    public static void Log(object msg) => AndroidUtils.Log.Debug(Tag, msg.ToString());
}
