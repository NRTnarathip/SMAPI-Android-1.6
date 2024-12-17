using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Java.Util;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI.Mobile;

internal static class AndroidGameLoopManager
{
    internal delegate bool OnGameUpdatingDelegate(GameTime gameTime);
    static List<OnGameUpdatingDelegate> listOnGameUpdating = new();
    static Queue<OnGameUpdatingDelegate> queueOnGameUpdatingToAdd = new();
    static Queue<OnGameUpdatingDelegate> queueOnGameUpdatingToRemove = new();

    /// <summary>
    /// Register On Main Thread Only!!
    /// </summary>
    /// <param name="onGameUpdate"></param>
    internal static void RegisterOnGameUpdating(OnGameUpdatingDelegate onGameUpdate)
    {
        queueOnGameUpdatingToAdd.Enqueue(onGameUpdate);
    }

    /// <summary>
    /// Unregister On Main Thread Only!!
    /// </summary>
    /// <param name="onGameUpdate"></param>
    internal static void UnregisterOnGameUpdating(OnGameUpdatingDelegate onGameUpdate)
    {
        queueOnGameUpdatingToRemove.Enqueue(onGameUpdate);
    }

    public static bool IsSkipOriginalGameUpdating { get; private set; } = false;
    internal static void OnGameUpdating(GameTime gameTime)
    {
        //reset
        IsSkipOriginalGameUpdating = false;

        if (queueOnGameUpdatingToAdd.Count > 0)
        {
            while (queueOnGameUpdatingToAdd.TryDequeue(out OnGameUpdatingDelegate item))
            {
                if (listOnGameUpdating.Contains(item) is false)
                    listOnGameUpdating.Add(item);
            }
        }

        if (queueOnGameUpdatingToRemove.Count > 0)
        {
            while (queueOnGameUpdatingToRemove.TryDequeue(out OnGameUpdatingDelegate item))
            {
                if (listOnGameUpdating.Contains(item))
                    listOnGameUpdating.Remove(item);
            }
        }

        foreach (var callback in listOnGameUpdating)
        {
            if (callback(gameTime))
            {
                IsSkipOriginalGameUpdating = true;
            }
        }
    }
}
