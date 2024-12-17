using System;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI.Framework;
using StardewValley;
using StardewValley.Audio;

namespace StardewModdingAPI.Mobile.Facade;

public static class StardewAudioMethods
{
    public static FieldInfo _categories_Field = AccessTools.Field(typeof(AudioEngine), "_categories");

    internal static MethodInfo IAudioEngine_GetCategoryIndex_MI
        = AccessTools.Method(typeof(StardewAudioMethods), nameof(IAudioEngine_GetCategoryIndex));

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
    public static int IAudioEngine_GetCategoryIndex(this IAudioEngine obj, string name)
    {
        //Console.WriteLine("On GetCategoryIndex()");
        //Console.WriteLine("obj: " + obj);
        //Console.WriteLine("name: " + name);
        switch (obj)
        {
            case AudioEngine audioEngine:
                return audioEngine.GetCategoryIndex(name);

            case AudioEngineWrapper audioEngineWrapper:
                return audioEngineWrapper.AudioEngineWrapper_GetCategoryIndex(name);

            case DummyAudioEngine dummyAudioEngine:
                return -1;

            default:
                //called method on base type of object
                var GetCategoryIndex_Method = AccessTools.Method(obj.GetType(), "GetCategoryIndex");
                return (int)GetCategoryIndex_Method.Invoke(obj, [name]);
        }
    }


    static FieldInfo soundBank_FI = AccessTools.Field(typeof(SoundBankWrapper), "soundBank");
    internal static void SoundBankWrapper_AddCue(this SoundBankWrapper soundBankWrapper, CueDefinition cue)
    {
        var soundBank = soundBank_FI.GetValue(soundBankWrapper) as SoundBank;
        soundBank.AddCue(cue);
    }

    internal static MethodInfo ISoundBank_AddCue_MI
        = AccessTools.Method(typeof(StardewAudioMethods), nameof(ISoundBank_AddCue));
    public static void ISoundBank_AddCue(this ISoundBank obj, CueDefinition cue)
    {
        //Console.WriteLine("On ISoundBank_AddCue()");
        //Console.WriteLine("obj: " + obj);
        switch (obj)
        {
            case SoundBank soundBank:
                soundBank.AddCue(cue);
                break;

            case SoundBankWrapper soundBankWrapper:
                SoundBankWrapper_AddCue(soundBankWrapper, cue);
                break;

            default:
                //called method on base type of object
                var monitor = SCore.Instance.GetMonitorForGame();
                AccessTools.Method(obj.GetType(), "AddCue").Invoke(obj, [cue]);
                break;
        }
    }
}
