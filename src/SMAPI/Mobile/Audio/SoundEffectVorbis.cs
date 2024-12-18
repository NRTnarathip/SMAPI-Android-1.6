using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using NVorbis;
using NVorbis.Contracts;

namespace StardewModdingAPI.Mobile.Facade;

public class SoundEffectVorbis : SoundEffect
{
    public SoundEffectVorbis(byte[] buffer, int sampleRate, AudioChannels channels) : base(buffer, sampleRate, channels)
    {
    }

    public SoundEffectVorbis(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength) : base(buffer, offset, count, sampleRate, channels, loopStart, loopLength)
    {
    }
    public static SoundEffectVorbis CreateFromFileStream(string soundFilePath, bool vorbis)
    {
        var st = Stopwatch.StartNew();
        using FileStream stream = new FileStream(soundFilePath, FileMode.Open);
        var sound = AccessTools.CreateInstance<SoundEffectVorbis>();

        AccessTools.Method(typeof(SoundEffect), "Initialize").Invoke(sound, null);
        var _duration_FI = AccessTools.Field(typeof(SoundEffect), "_duration");
        Console.WriteLine("starting load sound vorbis: " + soundFilePath);

        using (VorbisReader vorbis_reader = new VorbisReader(stream, closeOnDispose: true))
        {

            const int bytes_per_sample = 2;

            float[] float_buffer = new float[vorbis_reader.TotalSamples * vorbis_reader.Channels];
            short[] cast_buffer = new short[float_buffer.Length];
            byte[] xna_buffer = new byte[float_buffer.Length * bytes_per_sample];
            int read_samples = vorbis_reader.ReadSamples(float_buffer, 0, float_buffer.Length);
            OggStream.CastBuffer(float_buffer, cast_buffer, read_samples);
            Buffer.BlockCopy(cast_buffer, 0, xna_buffer, 0, read_samples * bytes_per_sample);

            //debug
            //ReadSamples(vorbis_reader, out byte[] xna_buffer);

            _duration_FI.SetValue(sound, vorbis_reader.TotalTime);
            //Console.WriteLine("done _duration setvalue: " + vorbis_reader.TotalTime);

            var PlatformInitializePcm = AccessTools.Method(typeof(SoundEffect), "PlatformInitializePcm");
            PlatformInitializePcm.Invoke(sound, [
                xna_buffer, 0, xna_buffer.Length, 16,
                    vorbis_reader.SampleRate,
                    (AudioChannels)vorbis_reader.Channels, 0,
                    (int)vorbis_reader.TotalSamples
            ]);
        }

        Console.WriteLine($"created SoundEffectVorbis in {st.Elapsed.TotalSeconds}s");
        return sound;
    }
}
