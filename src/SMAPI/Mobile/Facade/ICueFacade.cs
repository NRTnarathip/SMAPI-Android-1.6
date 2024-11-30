using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI.Framework.ModLoading.Rewriters;
using StardewValley;

namespace StardewModdingAPI.Mobile.Facade;

internal class ICueFacade : ICue, IRewriteFacade
{
    public bool IsStopped => throw new NotImplementedException();

    public bool IsStopping => throw new NotImplementedException();

    public bool IsPlaying => throw new NotImplementedException();

    public bool IsPaused => throw new NotImplementedException();

    public string Name => throw new NotImplementedException();

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public float GetVariable(string var)
    {
        throw new NotImplementedException();
    }

    public void Pause()
    {
        throw new NotImplementedException();
    }

    public void Play()
    {
        throw new NotImplementedException();
    }

    public void Resume()
    {
        throw new NotImplementedException();
    }

    public void SetVariable(string var, int val)
    {
        throw new NotImplementedException();
    }

    public void SetVariable(string var, float val)
    {
        throw new NotImplementedException();
    }

    public void Stop(AudioStopOptions options)
    {
        throw new NotImplementedException();
    }
}
