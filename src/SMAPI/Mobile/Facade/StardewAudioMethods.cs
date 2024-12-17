using System;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using StardewValley.Audio;

namespace StardewModdingAPI.Mobile.Facade;

public static class StardewAudioMethods
{
    public static FieldInfo _categories_Field = AccessTools.Field(typeof(AudioEngine), "_categories");

    internal static MethodInfo IAudioEngine_GetCategoryIndex_MethodInfo =
        AccessTools.Method(typeof(StardewAudioMethods), nameof(IAudioEngine_GetCategoryIndex));

    internal static int AudioEngine_GetCategoryIndex(this AudioEngine audioEngine, string name)
    {
        Console.WriteLine("Try get GetCategoryIndex on AudioEngine: " + audioEngine);
        var _categories = _categories_Field.GetValue(audioEngine) as AudioCategory[];
        for (int i = 0; i < _categories.Length; i++)
        {
            if (_categories[i].Name == name)
            {
                return i;
            }
        }
        return -1;
    }
    internal static int AudioEngineWrapper_GetCategoryIndex(this AudioEngineWrapper audioEngineWrapper, string name)
    {
        Console.WriteLine("Try get GetCategoryIndex on AudioEngineWrapper: " + audioEngineWrapper);
        try
        {
            var engine = audioEngineWrapper.Engine;
            Console.WriteLine("engine: " + engine);
            int index = AudioEngine_GetCategoryIndex(engine, name);
            Console.WriteLine("result index: " + index);
            return index;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        return -1;
    }
    public static int IAudioEngine_GetCategoryIndex(this object obj, string name)
    {
        Console.WriteLine("On GetCategoryIndex()");
        Console.WriteLine("obj: " + obj);
        Console.WriteLine("name: " + name);

        switch (obj)
        {
            case AudioEngine audioEngine:
                return audioEngine.GetCategoryIndex(name);

            case AudioEngineWrapper audioEngineWrapper:
                return audioEngineWrapper.AudioEngineWrapper_GetCategoryIndex(name);

            default:
                return -1;
        }
    }


}
