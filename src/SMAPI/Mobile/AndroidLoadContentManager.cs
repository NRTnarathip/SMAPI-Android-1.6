using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewModdingAPI.Framework;
using StardewValley;

namespace StardewModdingAPI.Mobile;

internal static class AndroidLoadContentManager
{
    public static bool IsLoaded => LoadState == LoadStateEnum.Loaded;

    public static bool FinishedFirstInitSerializers
    {
        get => (bool)AccessTools.Field(typeof(Game1), "FinishedFirstInitSerializers").GetValue(null);
        set => AccessTools.Field(typeof(Game1), "FinishedFirstInitSerializers").SetValue(null, value);
    }
    public static bool FinishedFirstLoadContent
    {
        get => (bool)AccessTools.Field(typeof(Game1), "FinishedFirstLoadContent").GetValue(null);
        set => AccessTools.Field(typeof(Game1), "FinishedFirstLoadContent").SetValue(null, value);
    }

    public static bool FinishedFirstInitSounds
    {
        get => (bool)AccessTools.Field(typeof(Game1), "FinishedFirstInitSounds").GetValue(null);
        set => AccessTools.Field(typeof(Game1), "FinishedFirstInitSounds").SetValue(null, value);
    }

    public static void UpdateMoveNextLoadContent()
    {
        if (IsLoaded)
            return;

        var loadEnumerator = SGame.LoadContentEnumerator;
        //Console.WriteLine("current state: " + loadEnumerator.Current);
        if (loadEnumerator.Current == 0)
        {
            LoadState = LoadStateEnum.Loading;
        }
        bool isLoadContentFinish = loadEnumerator.MoveNext() is false;
        if (isLoadContentFinish)
        {
            FinishedFirstLoadContent = true;
        }

        if (FinishedFirstLoadContent && FinishedFirstInitSounds && FinishedFirstInitSerializers)
        {
            AccessTools.Field(typeof(Game1), "FinishedIncrementalLoad").SetValue(null, true);
            LoadState = LoadStateEnum.Loaded;
            SGame.LoadContentEnumerator = null;
            AccessTools.Method(typeof(Game1), "AfterLoadContent").Invoke(Game1.game1, null);
            Console.WriteLine("End called AfterLoadContent");
            (SGame.game1 as SGame).OnAndroidContentLoaded();
        }

        //Console.WriteLine("after move next current step: " + loadEnumerator.Current);
        //Console.WriteLine("is loaded: " + IsLoaded);
    }
    public enum LoadStateEnum
    {
        None,
        Loading,
        Loaded,
    }
    public static LoadStateEnum LoadState = LoadStateEnum.None;
}
