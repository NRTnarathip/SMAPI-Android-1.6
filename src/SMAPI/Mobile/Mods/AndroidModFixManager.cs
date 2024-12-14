using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI.Mobile;

internal class AndroidModFixManager
{
    Dictionary<string, Action<Assembly>> OnModLoadedRegistry = new();
    public static AndroidModFixManager Instance { get; private set; }
    AndroidModFixManager()
    {
        Instance = this;
    }

    public static AndroidModFixManager Init()
    {
        Instance = new();
        AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
        return Instance;
    }

    static void CurrentDomain_AssemblyLoad(object? sender, AssemblyLoadEventArgs args)
    {
        Instance.OnAsmLoad(args.LoadedAssembly);
    }

    private void OnAsmLoad(Assembly asm)
    {
        string name = asm.GetName().Name;
        if (this.OnModLoadedRegistry.TryGetValue(name, out Action<Assembly> cb))
        {
            try
            {
                cb.Invoke(asm);
            }
            catch (Exception ex)
            {
                var monitor = SCore.Instance.GetMonitorForGame();
                monitor.Log(ex.ToString(), LogLevel.Error);
            }
            this.OnModLoadedRegistry.Remove(name);
        }
    }

    public void RegisterOnModLoaded(string asmNameOrFileName, Action<Assembly> callback)
    {
        string nameWithoutExtension = asmNameOrFileName.Replace(".dll", "");
        this.OnModLoadedRegistry.TryAdd(nameWithoutExtension, callback);
    }

    //key: AssemblyName, value: callback
    Dictionary<string, Action<Mono.Cecil.AssemblyDefinition>> OnRewriteModDictionary = new();
    internal void RegisterRewriteModAssemblyDef(string v, Action<Mono.Cecil.AssemblyDefinition> callback)
    {
        this.OnRewriteModDictionary.TryAdd(v, callback);
    }

    internal void TryRewriteMod(Framework.ModLoading.AssemblyParseResult assembly, out bool hasRewrite, out Exception exception)
    {
        string assemblyName = assembly.Definition.Name.Name;
        hasRewrite = false;
        exception = null;

        if (this.OnRewriteModDictionary.TryGetValue(assemblyName, out var callback))
        {
            var monitor = SCore.Instance.GetMonitorForGame();
            this.OnRewriteModDictionary.Remove(assemblyName);

            monitor.Log("Try ModFixManager rewrite mod: " + assembly.Definition.Name);
            try
            {
                callback.Invoke(assembly.Definition);
                hasRewrite = true;
                monitor.Log("Done rewrite mod: " + assembly.Definition.Name);
            }
            catch (Exception ex)
            {
                monitor.Log(ex.ToString(), LogLevel.Error);
                exception = ex;
            }
        }
    }
}