using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI.Framework;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.GameData;

namespace StardewModdingAPI.Mobile.Facade;

[HarmonyPatch]
public static class StardewAudioMethods
{

    public static FieldInfo _categories_Field = AccessTools.Field(typeof(AudioEngine), "_categories");

    internal static MethodInfo IAudioEngine_GetCategoryIndex_MI
        = AccessTools.Method(typeof(StardewAudioMethods), nameof(IAudioEngine_GetCategoryIndex));

    internal static int AudioEngine_GetCategoryIndex(this AudioEngine audioEngine, string name)
    {
        //Console.WriteLine("Try get GetCategoryIndex on AudioEngine: " + audioEngine);
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
        //Console.WriteLine("Try get GetCategoryIndex on AudioEngineWrapper: " + audioEngineWrapper);
        try
        {
            var engine = audioEngineWrapper.Engine;
            //Console.WriteLine("engine: " + engine);
            int index = AudioEngine_GetCategoryIndex(engine, name);
            //Console.WriteLine("result index: " + index);
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


    #region SoundBank
    static FieldInfo soundBank_FI = AccessTools.Field(typeof(SoundBankWrapper), "soundBank");
    internal static void SoundBankWrapper_AddCue(this SoundBankWrapper soundBankWrapper, CueDefinition cue)
    {
        soundBankWrapper.GetSoundBank().AddCue(cue);
    }
    internal static SoundBank GetSoundBank(this SoundBankWrapper soundBankWrapper)
        => soundBank_FI.GetValue(soundBankWrapper) as SoundBank;

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

    #endregion


    #region ICue Rewriter

    //TODO
    //SoundHelper PlayLocal it don't use cue.Volume
    static PropertyInfo ICue_Volume_PI = AccessTools.Property(typeof(ICue), "Volume");

    static Dictionary<ICue, float> holder_Volume = new();
    internal const string get_Volume_FullName = "System.Single StardewValley.ICue::get_Volume()";
    internal static MethodInfo ICue_get_Volume_MI = AccessTools.Method(typeof(StardewAudioMethods), nameof(ICue_get_Volume_MI));
    internal static float ICue_get_Volume(this ICue icue)
    {
        //Console.WriteLine("On ICue_get_Volume");
        //Console.WriteLine("cue: " + icue);
        switch (icue)
        {
            case CueWrapper cue:
                //Console.WriteLine("cue wrapper: " + cue.Name);
                break;

            case DummyCue dummy:
                //Console.WriteLine("dummy cue: " + dummy.Name);
                break;

            default:
                break;
        }
        if (holder_Volume.TryGetValue(icue, out float volume) is false)
        {
            volume = 1f;
            icue.ICue_set_Volume(volume);
            //Console.WriteLine("done first apply ICue_set_Volume");
        }
        //Console.WriteLine("ICue_get_Volume volume: " + volume);
        return volume;
    }

    internal const string set_Volume_FullName = "System.Void StardewValley.ICue::set_Volume(System.Single)";
    internal static MethodInfo ICue_set_Volume_MI = AccessTools.Method(typeof(StardewAudioMethods), nameof(ICue_set_Volume));
    internal static void ICue_set_Volume(this ICue icue, float newValue)
    {
        //Console.WriteLine("On ICue_set_Volume");
        //Console.WriteLine("cue: " + icue);
        //Console.WriteLine("  name: " + icue.Name);
        //Console.WriteLine("  volume value: " + newValue);
        holder_Volume[icue] = newValue;
    }

    internal static void Init(AndroidModFixManager modFix)
    {
        //AndroidGameLoopManager.RegisterOnGameUpdating(OnGameUpdating);
    }

    private static bool OnGameUpdating(GameTime gameTime)
    {
        var soundBankWrapper = Game1.soundBank as SoundBankWrapper;
        if (soundBankWrapper is null)
            return false;

        if (SCore.ProcessTicksElapsed % 60 != 0)
            return false;

        var soundBank = soundBankWrapper.GetSoundBank();
        var cues = AccessTools.Field(typeof(SoundBank), "_cues")
            .GetValue(soundBank) as Dictionary<string, CueDefinition>;

        Console.WriteLine("cues len: " + cues.Count);

        foreach (var cueParItem in cues)
        {
            var cue = cueParItem.Value;
            Console.WriteLine("- cue name: " + cue.name);
            Console.WriteLine("- sound len: " + cue.sounds.Count);
        }

        return false;
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(AudioCueModificationManager), nameof(AudioCueModificationManager.ApplyCueModification))]
    static bool Prefix_ApplyCueModification(AudioCueModificationManager __instance, string key)
    {
        try
        {
            Console.WriteLine("On Prefix_ApplyCueModification() key:  " + key);
            var cueModificationData = __instance.cueModificationData;

            if (!cueModificationData.TryGetValue(key, out var modification_data))
            {
                return false;
            }
            bool is_modification = false;
            int category_index = Game1.audioEngine.IAudioEngine_GetCategoryIndex("Default");
            CueDefinition cue_definition;
            var soundBankWrapper = Game1.soundBank as SoundBankWrapper;
            var soundBank = soundBankWrapper.GetSoundBank();

            if (soundBank.Exists(modification_data.Id))
            {
                cue_definition = soundBank.GetCueDefinition(modification_data.Id);
                is_modification = true;
            }
            else
            {
                cue_definition = new CueDefinition();
                cue_definition.name = modification_data.Id;
            }
            if (modification_data.Category != null)
            {
                category_index = Game1.audioEngine.IAudioEngine_GetCategoryIndex(modification_data.Category);
            }
            if (modification_data.FilePaths != null)
            {
                SoundEffect[] effects = new SoundEffect[modification_data.FilePaths.Count];
                for (int i = 0; i < modification_data.FilePaths.Count; i++)
                {
                    string file_path = __instance.GetFilePath(modification_data.FilePaths[i]);
                    bool vorbis = Path.GetExtension(file_path).EqualsIgnoreCase(".ogg");
                    int invalid_sounds = 0;
                    try
                    {
                        SoundEffect sound_effect;
                        if (vorbis && modification_data.StreamedVorbis)
                        {
                            sound_effect = OggStreamSoundEffect.CreateOggStreamFromFileName(file_path);
                        }
                        else
                        {
                            using FileStream stream = new FileStream(file_path, FileMode.Open);
                            sound_effect = SoundEffect.FromStream(stream, vorbis);
                        }
                        effects[i - invalid_sounds] = sound_effect;
                    }
                    catch (Exception e)
                    {
                        Game1.log.Error("Error loading sound: " + file_path, e);
                        invalid_sounds++;
                    }
                    if (invalid_sounds > 0)
                    {
                        Array.Resize(ref effects, effects.Length - invalid_sounds);
                    }
                }
                cue_definition.SetSound(effects, category_index, modification_data.Looped, modification_data.UseReverb);
                if (is_modification)
                {
                    cue_definition.OnModified?.Invoke();
                }
            }
            soundBank.AddCue(cue_definition);
        }
        catch (NoAudioHardwareException)
        {
            Game1.log.Warn("Can't apply modifications for audio cue '" + key + "' because there's no audio hardware available.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }


        return false;
    }


    #endregion
}
