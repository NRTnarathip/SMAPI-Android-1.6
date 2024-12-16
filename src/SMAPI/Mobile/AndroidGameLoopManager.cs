using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI.Mobile;

internal static class AndroidGameLoopManager
{
    internal static Action<GameTime>? eventOnGameUpdating = null;
    internal static void RestoreOnGameUpdating()
    {
        eventOnGameUpdating = null;
        SGameRunner.Instance.OnGameUpdating = SCore.Instance.OnGameUpdating;
    }

    internal static void SetOnGameUpdating(Action<GameTime> newOnGameUpdating)
    {
        eventOnGameUpdating = newOnGameUpdating;
        SGameRunner.Instance.OnGameUpdating = NewGameUpdating;
    }
    internal static void NewGameUpdating(GameTime time, Action runCallback)
    {
        eventOnGameUpdating.Invoke(time);
    }
}
