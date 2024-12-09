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
    Dictionary<string, Action<Mono.Cecil.AssemblyDefinition>> OnDoneRewriterModDictionary = new();
    internal void RegisterOnRewriterModAssemblyDef(string v, Action<Mono.Cecil.AssemblyDefinition> callback)
    {
        this.OnDoneRewriterModDictionary.TryAdd(v, callback);
    }

    internal void OnDoneRewriterMod(Framework.ModLoading.AssemblyParseResult assembly)
    {
        string assemblyName = assembly.Definition.Name.Name;
        if (this.OnDoneRewriterModDictionary.TryGetValue(assemblyName, out var callback))
        {
            try
            {
                callback.Invoke(assembly.Definition);
            }
            catch (Exception ex)
            {
                var monitor = SCore.Instance.GetMonitorForGame();
                monitor.Log(ex.ToString(), LogLevel.Error);
            }
            //clean up
            this.OnDoneRewriterModDictionary.Remove(assemblyName);
        }
    }
}
