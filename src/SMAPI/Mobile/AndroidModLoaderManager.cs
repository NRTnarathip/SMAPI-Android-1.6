using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Android.Systems;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.Logging;
using StardewValley;
using static System.Net.Mime.MediaTypeNames;
using static Java.Util.Jar.Attributes;
using static StardewValley.BellsAndWhistles.PlayerStatusList;

namespace StardewModdingAPI.Mobile;

internal static class AndroidModLoaderManager
{
    public enum LoadStatus
    {
        None = 0,
        Starting = 1,
        LoadedAndNeedToConfirm = 2,
        LoadedConfirm = 3,
    }
    static object _lock = new();
    static LoadStatus _loadStatus = LoadStatus.None;
    public static LoadStatus CurrentStatus
    {
        get
        {
            lock (_lock)
            {
                return _loadStatus;
            }
        }
        set
        {
            lock (_lock)
            {
                _loadStatus = value;
            }
        }
    }


    static SpriteFont smallFont;
    static LocalizedContentManager content;
    static List<string> logLines = new();
    static float K_textLineHeight;
    static object _lock_logLines = new object();
    internal static void StartLoggerToScreen()
    {
        LogFileManager.OnWriteLine += OnWriteLine;
        content = Game1.game1.CreateContentManager(Game1.content.ServiceProvider, Game1.content.RootDirectory);
        smallFont = content.Load<SpriteFont>("Fonts\\SmallFont");
        K_textLineHeight = smallFont.MeasureString("AAA").Y;
        logLines.Add("Hello World");
        logLines.Add("Hello World 2");
    }

    internal static void StopLoggerToScreen()
    {
        //LogFileManager.OnWriteLine -= OnWriteLine;
        //logLines.Clear();
    }

    internal static void TickUpdate()
    {
        //wait thread mod loader
        Task? taskModEntry;
        lock (_lock_queueTaskStartModEntry)
        {
            queueTaskStartModEntry.TryDequeue(out taskModEntry);
        }
        if (taskModEntry != null)
        {
            try
            {
                Console.WriteLine($"taskModEntry.RunSynchronously(); ID: {taskModEntry.Id} on Main Thread");
                taskModEntry.RunSynchronously();
                Console.WriteLine("End taskModEntry.RunSynchronously() in main thread");
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception on task: " + ex);
            }
        }
    }
    static Queue<Task> queueTaskStartModEntry = new();
    static object _lock_queueTaskStartModEntry = new();
    internal static void TryStartModEntry(IMod mod)
    {
        Task taskModEntry = new Task(() =>
        {
            mod.Entry(mod.Helper);
        });

        lock (_lock_queueTaskStartModEntry)
        {
            queueTaskStartModEntry.Enqueue(taskModEntry);
        }

        //wait
        Console.WriteLine("task id: " + taskModEntry.Id + ", mod name: " + mod.GetType());
        Console.WriteLine("taskModEntry.Wait()...");
        try
        {
            taskModEntry.Wait();
        }
        catch (Exception ex)
        {
            throw taskModEntry.Exception;
        }
        finally
        {

            Console.WriteLine("finally taskModEntry.Wait()");
            //Console.WriteLine(" task exception: " + taskModEntry.Exception);
            //Console.WriteLine(" task IsCompleted: " + taskModEntry.IsCompleted);
            //Console.WriteLine(" task IsCompletedSuccessfully: " + taskModEntry.IsCompletedSuccessfully);
            //Console.WriteLine(" task IsFaulted: " + taskModEntry.IsFaulted);
            //Console.WriteLine(" task IsCanceled: " + taskModEntry.IsCanceled);
        }
    }


    //unsafe thread
    static void OnWriteLine(LogFileManager manager, string msg)
    {
        lock (_lock_logLines)
        {
            logLines.Add(msg);
        }
    }

    internal static void Draw(GameTime gameTime, RenderTarget2D target_screen)
    {
        var spriteBatch = Game1.spriteBatch;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

        int lineCount;
        lock (_lock_logLines)
        {
            lineCount = logLines.Count;
        }

        var viewport = Game1.viewport;
        Game1.game1.GraphicsDevice.Clear(Color.Black);
        for (int i = 0; i < lineCount; i++)
        {
            //TODO: not sure is for safe access
            string text;
            lock (_lock_logLines)
            {
                text = logLines[lineCount - i - 1];
            }

            //draw from Left, Bottom
            const float K_fontScale = 1.3f;
            Vector2 pos = Vector2.Zero;
            const int startYPadding = 30;
            float lineHeight = K_fontScale * K_textLineHeight;
            pos.Y = viewport.Height - (startYPadding + lineHeight + (lineHeight * i));
            pos.X = 20;


            //bug not works
            //if you have use harmony patching method between mod load entry point
            //such as mod Thai Font Adjuster
            //so we should modify ModLoader at mod.Entry() with sync main thread
            spriteBatch.DrawString(smallFont, text, pos, Color.White,
                0f, Vector2.Zero, K_fontScale, SpriteEffects.None, 10);

        }

        spriteBatch.End();
    }

}
