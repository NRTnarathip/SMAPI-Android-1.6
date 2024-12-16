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
    internal static bool OnGameUpdating_TaskUpdate(GameTime time)
    {

#if false
        //debug only
        if (SCore.ProcessTicksElapsed % 30 == 0)
        {
            Console.WriteLine();
            Console.WriteLine("wait mod hook task...");
            foreach (var task in tasks)
            {
                Console.WriteLine($"status task ID: {task.Id}");
                Console.WriteLine($"  status: {task.Status}");
                Console.WriteLine($"  IsCanceled: {task.IsCanceled}");
                Console.WriteLine($"  IsCompleted: {task.IsCompleted}");
                Console.WriteLine($"  IsCompletedSuccessfully: {task.IsCompletedSuccessfully}");
                Console.WriteLine($"  IsFaulted: {task.IsFaulted}");
            }
        }
#endif

        if (tasks.Count > 0)
        {
            int removeCount = tasks.RemoveAll(task => task.IsCompleted);
        }

        //done
        if (tasks.Count == 0)
            return false;

        return true;
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

        //ready
        currentModHookTask.Start();

        Console.WriteLine($"End & return StartTask name: '{nameID}', taskIDNumber: {currentModHookTask.Id}");
        return currentModHookTask;
    }

    internal static void Init()
    {
        AndroidGameLoopManager.RegisterOnGameUpdating(OnGameUpdating_TaskUpdate);
    }
}
