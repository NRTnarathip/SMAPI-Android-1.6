using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            cb.Invoke(asm);

    }

    public void RegisterOnModLoaded(string asmNameOrFileName, Action<Assembly> callback)
    {
        string nameWithoutExtension = asmNameOrFileName.Replace(".dll", "");
        this.OnModLoadedRegistry.TryAdd(nameWithoutExtension, callback);
    }
}
