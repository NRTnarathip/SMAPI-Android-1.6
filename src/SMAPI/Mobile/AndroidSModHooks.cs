using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Internal;
using StardewValley.Extensions;

namespace StardewModdingAPI.Mobile;

[HarmonyPatch]
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

        bool markSkipGameUpdating = false;

        //process task on main thread
        //update main thread task

        // millisecond 1000.0 == 1 sec
        double runTaskOnMainThreadTotalTime = 0;
        int runTaskOnMainThreadCount = 0;
        while (queueTaskOnMainThread.TryDequeue(out var task))
        {
            markSkipGameUpdating = true;
            var st = Stopwatch.StartNew();
            Console.WriteLine("Start taskOnMainThread: " + task.name);
            task.task.RunSynchronously();
            st.Stop();
            runTaskOnMainThreadCount++;
            runTaskOnMainThreadTotalTime += st.Elapsed.TotalMilliseconds;
            Console.WriteLine($"Done taskOnMainThread taskName: {task.name} in {st.Elapsed.TotalMilliseconds}ms");
            Console.WriteLine("current total time: " + runTaskOnMainThreadTotalTime);

            //limit total time, prevent ANR
            if (runTaskOnMainThreadTotalTime > 500)
                break;
        }

        //process task background thread
        if (listTaskOnThreadBackground.Count > 0)
        {
            int removeCount = listTaskOnThreadBackground.RemoveAll(task => task.IsCompleted);
        }
        if (listTaskOnThreadBackground.Count > 0)
        {
            markSkipGameUpdating = true;
        }

        return markSkipGameUpdating;
    }
    internal class TaskOnMainThread
    {
        public readonly string name;
        public readonly Task task;
        public TaskOnMainThread(Task task, string name)
        {
            this.task = task;
            this.name = name;
        }
    }
    static List<Task> listTaskOnThreadBackground = new();
    static ConcurrentQueue<TaskOnMainThread> queueTaskOnMainThread = new();

    internal static Task AddTaskRunOnMainThread(Task yourTask, string name)
    {
        var taskOnMainThread = new TaskOnMainThread(yourTask, name);
        queueTaskOnMainThread.Enqueue(taskOnMainThread);
        Console.WriteLine("Add task OnMainThrad name: " + name);
        return yourTask;
    }
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

        Console.WriteLine("try add new task, current task count: " + listTaskOnThreadBackground.Count);
        listTaskOnThreadBackground.Add(currentModHookTask);

#if false
        //debug only
        currentModHookTask.Start();
#else
        //ready
        currentModHookTask.Start();
#endif

        Console.WriteLine($"End & return StartTask name: '{nameID}', taskIDNumber: {currentModHookTask.Id}");
        return currentModHookTask;
    }

    internal static void Init()
    {
        AndroidGameLoopManager.RegisterOnGameUpdating(OnGameUpdating_TaskUpdate);
    }
}
