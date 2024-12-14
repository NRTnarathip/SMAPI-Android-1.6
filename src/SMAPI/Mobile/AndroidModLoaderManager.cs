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
using StardewModdingAPI.Internal.ConsoleWriting;
using StardewValley;
using static System.Net.Mime.MediaTypeNames;
using static Android.Renderscripts.ScriptGroup;
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

    static object _lock_logLines = new object();
    internal static void StartLoggerToScreen()
    {
        StardewModdingAPI.Framework.Monitor.RegisterOnLogImpl(OnLogImpl);
        content = Game1.game1.CreateContentManager(Game1.content.ServiceProvider, Game1.content.RootDirectory);
        smallFont = content.Load<SpriteFont>("Fonts\\SmallFont");
        K_textLineHeight = smallFont.MeasureString("AAA").Y;
    }

    static bool IsStopLogger = false;
    internal static void StopLoggerToScreen()
    {
        StardewModdingAPI.Framework.Monitor.UnregisterOnLogImpl(OnLogImpl);
        IsStopLogger = true;
        Console.WriteLine("stop mod loader logger");
    }

    static void OnLogImpl(ConsoleLogLevel logLevel, string msg)
    {
        lock (_lock_logLines)
        {
            //split lines
            string[] lines = msg.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            foreach (string line in lines)
                logLines.Add($"<ConsoleLogLevel>{logLevel}</ConsoleLogLevel><line>{line}</line>");
        }
    }

    internal static void Draw(GameTime gameTime, RenderTarget2D target_screen)
    {
        if (IsStopLogger)
            return;

        Game1.PushUIMode();
        var spriteBatch = Game1.spriteBatch;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

        int lineCount;
        lock (_lock_logLines)
        {
            lineCount = logLines.Count;
        }

        var viewport = Game1.viewport;
        Game1.game1.GraphicsDevice.Clear(Color.Black);
        Color lineColor = Color.White;
        LogLevel currentLogLevel = LogLevel.Trace;
        for (int lineIndex = 0; lineIndex < lineCount; lineIndex++)
        {
            //TODO: not sure is for safe access
            string lineData;
            lock (_lock_logLines)
            {
                lineData = logLines[lineCount - lineIndex - 1];
            }


            if (GetLoglevel(lineData, ref currentLogLevel))
            {
                //update new log level
                switch (currentLogLevel)
                {
                    case LogLevel.Trace:
                    case LogLevel.Info:
                        lineColor = Color.White;
                        break;
                    case LogLevel.Alert:
                        lineColor = new(155, 56, 255);
                        break;
                    case LogLevel.Warn:
                        lineColor = new(255, 146, 56);
                        break;
                    case LogLevel.Error:
                        lineColor = new(255, 56, 70);
                        break;
                    default:
                        break;

                }
            }

            //draw from Left, Bottom
            const float K_fontScale = 1.3f;
            Vector2 pos = Vector2.Zero;
            const int startYPadding = 30;
            float lineHeight = K_fontScale * K_textLineHeight;
            pos.Y = viewport.Height - (startYPadding + lineHeight + (lineHeight * lineIndex));
            pos.X = 100;

            //bug not works
            //if you have use harmony patching method between mod load entry point
            //such as mod Thai Font Adjuster
            //so we should modify ModLoader at mod.Entry() with sync main thread
            string lineText = lineData[(lineData.IndexOf("<line>") + 6)..lineData.IndexOf("</line>")];
            spriteBatch.DrawString(smallFont, lineText, pos, lineColor,
                0f, Vector2.Zero, K_fontScale, SpriteEffects.None, 10);

        }

        spriteBatch.End();
        Game1.PopUIMode();
    }
    static bool GetLoglevel(string lineData, ref LogLevel logLevel)
    {
        string logLevelText = lineData[(lineData.IndexOf("<ConsoleLogLevel>") + 17)..lineData.IndexOf("</ConsoleLogLevel>")];
        if (Enum.TryParse(typeof(LogLevel), logLevelText, ignoreCase: true, out object logLevelEnum))
        {
            logLevel = (LogLevel)logLevelEnum;
            return true;
        }
        return false;
    }

}
