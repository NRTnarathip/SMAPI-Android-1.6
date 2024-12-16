using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Java.Util;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI.Mobile;

internal static class AndroidGameLoopManager
{
    internal delegate void OnGameUpdatingDelegate(GameTime gameTime);
    static List<OnGameUpdatingDelegate> listOnGameUpdating = new();
    static List<OnGameUpdatingDelegate> queueOnGameUpdatingToAdd = new();
    static List<OnGameUpdatingDelegate> queueOnGameUpdatingToRemove = new();

    internal static void RegisterOnGameUpdating(OnGameUpdatingDelegate onGameUpdate)
    {
        queueOnGameUpdatingToAdd.Add(onGameUpdate);
    }

    internal static void RemoveOnGameUpdating(OnGameUpdatingDelegate onGameUpdate)
    {
        queueOnGameUpdatingToRemove.Add(onGameUpdate);
    }

    public static bool IsSkipOriginalGameUpdating = true;
    internal static void OnGameUpdating(GameTime gameTime)
    {
        //reset
        IsSkipOriginalGameUpdating = false;
        if (queueOnGameUpdatingToAdd.Count > 0)
        {
            foreach (var item in queueOnGameUpdatingToAdd)
            {
                if (listOnGameUpdating.Contains(item) is false)
                    listOnGameUpdating.Add(item);
            }
        }


        if (queueOnGameUpdatingToRemove.Count > 0)
        {
            foreach (var item in queueOnGameUpdatingToRemove)
            {
                if (listOnGameUpdating.Contains(item))
                    listOnGameUpdating.Remove(item);
            }
        }


        if (listOnGameUpdating.Count > 0)
        {
            IsSkipOriginalGameUpdating = true;

            foreach (var callback in listOnGameUpdating)
            {
                callback(gameTime);
            }
        }
    }

}
