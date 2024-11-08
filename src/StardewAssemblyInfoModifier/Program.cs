using System.Reflection;
using System.Runtime.CompilerServices;
using dnlib.DotNet;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        const string stardewDllName = "StardewValley.dll";
        const string stardewGameDataDllName = "StardewValley.GameData.dll";

        const string assemblyDir = @"C:\Users\narat\Desktop\Stardew Valley Android\assemblies-reference-for-smapi";
        AddInternalsVisibleToSMAPI(Path.Combine(assemblyDir, stardewDllName));
        AddInternalsVisibleToSMAPI(Path.Combine(assemblyDir, stardewGameDataDllName));
        Console.ReadLine();
    }
    static void AddInternalsVisibleToSMAPI(string assemblyPath)
    {
        // Load the assembly
        ModuleDefMD module = ModuleDefMD.Load(assemblyPath);
        var attributeRef = module.CorLibTypes.GetTypeRef("System.Runtime.CompilerServices", "InternalsVisibleToAttribute");

        var ctor = new MemberRefUser(
            module,
            ".ctor",
            MethodSig.CreateInstance(module.CorLibTypes.Void, module.CorLibTypes.String),
            attributeRef);
        var internalsVisibleToAttr = new CustomAttribute(ctor);
        internalsVisibleToAttr.ConstructorArguments.Add(new CAArgument(module.CorLibTypes.String, "StardewModdingAPI"));
        module.Assembly.CustomAttributes.Add(internalsVisibleToAttr);
        module.Write(assemblyPath.Replace(".dll", ".modify.dll"));
        Console.WriteLine($"Added InternalsVisibleTo attribute to {assemblyPath} successfully.");
    }
}
