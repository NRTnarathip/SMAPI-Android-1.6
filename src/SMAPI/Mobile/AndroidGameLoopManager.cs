using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Java.Util;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Framework;
using StardewValley;

namespace StardewModdingAPI.Mobile;

[HarmonyPatch]
internal static class AndroidGameLoopManager
{
    internal delegate bool OnGameUpdatingDelegate(GameTime gameTime);
    static HashSet<OnGameUpdatingDelegate> listOnGameUpdating = new();
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
    internal static void UpdateFrame_OnGameUpdating(GameTime gameTime)
    {
        //reset
        IsSkipOriginalGameUpdating = false;

        if (queueOnGameUpdatingToAdd.Count > 0)
        {
            while (queueOnGameUpdatingToAdd.TryDequeue(out OnGameUpdatingDelegate item))
            {
                listOnGameUpdating.Add(item);
            }
        }

        if (queueOnGameUpdatingToRemove.Count > 0)
        {
            while (queueOnGameUpdatingToRemove.TryDequeue(out OnGameUpdatingDelegate item))
            {
                listOnGameUpdating.Remove(item);
            }
        }

        //Console.WriteLine("Android Looper OnGameUpdating...");
        foreach (var callback in listOnGameUpdating)
        {
            if (callback(gameTime))
            {
                IsSkipOriginalGameUpdating = true;
            }
        }
    }

    //fix game update freeze on GameTick & IsFixedTimeStep
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Game), "DoUpdate")]
    static void Postfix_DoUpdate(GameTime gameTime)
    {
        var game = SGameRunner.instance as Game;
        var _accumulatedElapsedTime_Field = AccessTools.Field(game.GetType(), "_accumulatedElapsedTime");
        var accumulatedElapsedTime = (TimeSpan)_accumulatedElapsedTime_Field.GetValue(game);

        if (accumulatedElapsedTime.TotalSeconds > 0.15f)
        {
            accumulatedElapsedTime = TimeSpan.FromSeconds(0f);
            _accumulatedElapsedTime_Field.SetValue(game, accumulatedElapsedTime);
            //release freeze loop Game.DoUpdate()
        }
    }
}
