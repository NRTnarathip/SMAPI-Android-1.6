using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Internal;
using StardewModdingAPI.Mobile.Facade;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Extensions;

namespace StardewModdingAPI.Mobile.Audio;

[HarmonyPatch]
internal class CustomAudioCueModificationManager : AudioCueModificationManager
{
    public static CustomAudioCueModificationManager Instance { get; private set; }
    IMonitor monitor;
    public CustomAudioCueModificationManager()
    {
        Instance = this;
        this.monitor = SCore.Instance.GetMonitorForGame();
    }

    [HarmonyPatch(typeof(AudioCueModificationManager))]
    [HarmonyPrefix]
    static bool Prefix_OnStartup()
    {
        return false;
    }


    void MyStartUp()
    {
        this.monitor.Log("Starting load cueModificationData...");
        this.cueModificationData = DataLoader.AudioChanges(Game1.content);
        this.ApplyAllCueModifications();
    }
    Task[] allTask_ApplyCueModification;
    public override void ApplyAllCueModifications()
    {
        try
        {
            Console.WriteLine("Start ApplyAllCueModifications count: " + this.cueModificationData.Count);

            this.allTask_ApplyCueModification = this.cueModificationData.Keys.Select(key =>
                Task.Run(() => this.ApplyCueModification(key))).ToArray();
        }
        catch (Exception ex)
        {
            this.monitor.Log(ex.GetLogSummary(), LogLevel.Error);
        }
    }

    public override void ApplyCueModification(string key)
    {
        try
        {
            var cueModificationData = this.cueModificationData;
            if (!cueModificationData.TryGetValue(key, out var modification_data))
            {
                return;
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
                    string file_path = this.GetFilePath(modification_data.FilePaths[i]);
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
                            //use SoundEffectVorbis Instead
                            //because SoundEffect.FromStream it don't have support voris on adnroid
                            sound_effect = SoundEffectVorbis.CreateFromFileStream(file_path, vorbis);
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
            //successfully
            this.cueLoadedDict.TryAdd(key, cue_definition);

        }
        catch (NoAudioHardwareException)
        {
            Game1.log.Warn("Can't apply modifications for audio cue '" + key + "' because there's no audio hardware available.");
        }
        catch (Exception ex)
        {
            this.monitor.Log(ex.ToString(), LogLevel.Error);
        }
    }
    ConcurrentDictionary<string, CueDefinition> cueLoadedDict = new();
    public bool IsLoaded { get; private set; }

    int tickUpdate = 0;
    internal void UpdateLoadContent(out bool isLoaded)
    {
        this.tickUpdate++;
        if (this.tickUpdate == 1)
        {
            //first tick init 
            this.MyStartUp();
        }

        //debug
        if (SCore.ProcessTicksElapsed % 30 == 0)
        {
            this.LogLoadingProgress();
        }

        //check all loaded
        if (this.cueLoadedDict.Count == this.cueModificationData.Count)
        {
            this.LogLoadingProgress();
            this.monitor.Log("Loaded CustomAudioCueModification");
            this.IsLoaded = true;
        }


        isLoaded = this.IsLoaded;
    }
    void LogLoadingProgress()
    {
        float percent = (this.cueLoadedDict.Count / (float)this.cueModificationData.Count) * 100;
        this.monitor.Log($"Loading cueModificationData {this.cueLoadedDict.Count}/{this.cueModificationData.Count} : {percent}%");
    }
}
