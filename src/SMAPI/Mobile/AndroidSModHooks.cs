using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Internal;
using StardewValley.Extensions;

namespace StardewModdingAPI.Mobile;

internal static class AndroidSModHooks
{
    static IMonitor Monitor => SCore.Instance.GetMonitorForGame();
    internal static void OnGameUpdating_TaskUpdate(GameTime time)
    {

#if false
        //debug only
        if (SCore.ProcessTicksElapsed % 30 == 0)
        {
            Console.WriteLine();
            Console.WriteLine("wait mod hook task...");
            foreach (var currentModHookTask in tasks)
            {
                Console.WriteLine($"status task ID: {currentModHookTask.Id}");
                Console.WriteLine($"  run time: {stopwatch.Elapsed.TotalMilliseconds}ms");
                Console.WriteLine($"  IsCanceled: {currentModHookTask.IsCanceled}");
                Console.WriteLine($"  IsCompleted: {currentModHookTask.IsCompleted}");
                Console.WriteLine($"  IsCompletedSuccessfully: {currentModHookTask.IsCompletedSuccessfully}");
                Console.WriteLine($"  IsFaulted: {currentModHookTask.IsFaulted}");
            }
        }
#endif

        tasks.RemoveWhere(task => task.IsCompleted);
        if (tasks.Count == 0)
        {
            AndroidGameLoopManager.RemoveOnGameUpdating(OnGameUpdating_TaskUpdate);
        }
    }
    static List<Task> tasks = new();
    internal static Task StartTask(Task gameTask, string nameID)
    {
        Monitor.Log($"Try StartTask name: '{nameID}' on Android SModHook");

        //setup
        var currentModHookTask = new Task(() =>
        {
            try
            {
                var st = Stopwatch.StartNew();
                Monitor.Log($"Starting Task id: '{nameID}'");
                gameTask.RunSynchronously();
                st.Stop();
                Monitor.Log($"Completed Task id: {nameID} in {st.Elapsed.TotalMilliseconds}ms");
            }
            catch (Exception ex)
            {
                Monitor.Log($"Exception on task id: {nameID}");
                Monitor.Log($"{ex.GetLogSummary()}");
            }
        });

        Console.WriteLine("try add new task, current task count: " + tasks.Count);
        tasks.Add(currentModHookTask);
        AndroidGameLoopManager.RegisterOnGameUpdating(OnGameUpdating_TaskUpdate);

        //ready
        currentModHookTask.Start();

        Console.WriteLine($"End & return StartTask name: '{nameID}', taskIDNumber: {currentModHookTask.Id}");
        return currentModHookTask;
    }
}
