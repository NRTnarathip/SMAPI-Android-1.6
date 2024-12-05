using System;
using System.IO;
using AndroidUtils = Android.Util;

namespace StardewModdingAPI.Mobile;

public static class AndroidLogger
{
    const string Tag = "SMAPI-Tag";
    public static void Log(object msg)
    {
        if (msg == null)
            msg = "";

        AndroidUtils.Log.Debug(Tag, msg.ToString());

        //TODO
        //Debug Only
        //LogToFile(msg.ToString());
    }

    static StreamWriter LogToFileStream;
    static string LogToFilePath = EarlyConstants.ExternalFilesDir + "/AndroidLog.txt";
    static void LogToFile(string msg)
    {
        //create first
        if (LogToFileStream == null)
        {
            if (File.Exists(LogToFilePath))
                File.Delete(LogToFilePath);

            LogToFileStream = new StreamWriter(LogToFilePath, append: true);
        }

        //ready
        LogToFileStream.WriteLine(msg);
        LogToFileStream.Flush();
    }
}
