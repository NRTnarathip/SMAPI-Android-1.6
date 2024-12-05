using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Lidgren.Network;
using LWJGL;
using MonoGame.Framework.Utilities;
using StardewValley.Network;

namespace StardewModdingAPI.Mobile;

internal static class LZ4LibLoaderFix
{
    public static void Init()
    {
        AndroidLogger.Log("Start Init LZ4Fix");
        try
        {
            object lwjgl_compressBound = AccessTools.Field(typeof(LZ4), "lwjgl_compressBound").GetValue(null);
            object lwjgl_compress_default = AccessTools.Field(typeof(LZ4), "lwjgl_compress_default").GetValue(null);
            object lwjgl_decompress_safe = AccessTools.Field(typeof(LZ4), "lwjgl_decompress_safe").GetValue(null);
            nint NativeLibrary = (nint)AccessTools.Field(typeof(LZ4), "NativeLibrary").GetValue(null);


            AndroidLogger.Log("try log static variables");
            AndroidLogger.Log(lwjgl_compressBound);
            AndroidLogger.Log(lwjgl_compress_default);
            AndroidLogger.Log(lwjgl_decompress_safe);
            AndroidLogger.Log(NativeLibrary);
        }
        catch (Exception ex)
        {
            AndroidLogger.Log(ex);
        }

        AndroidLogger.Log("Done Init LZ4Fix");
    }
}
