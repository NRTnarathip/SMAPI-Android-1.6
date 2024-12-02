using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.Framework.ModLoading.Framework;
using StardewModdingAPI.Framework.ModLoading.Rewriters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StardewModdingAPI.Mobile;

public sealed class MapMethodWithCallback
{
    public readonly string methodFullNameStartWith;
    public delegate bool CallbackDelegate(
        MapMethodWithCallback mapMethod,
        MethodReference methodReference,
        ModuleDefinition module,
        ILProcessor cil,
        Instruction instruction);

    public readonly CallbackDelegate callback;

    public MapMethodWithCallback(string methodFullNameStartWith, CallbackDelegate callback)
    {
        this.methodFullNameStartWith = methodFullNameStartWith;
        this.callback = callback;
    }
}


internal class MapMethodToStaticMethodRewriter : BaseInstructionHandler
{
    static string FormatCecilType(Type type)
    {
        return RewriteHelper.GetFullCecilName(type);
    }
    static string FormatCecilParameterList(ParameterInfo[] parameters)
    {
        var paramTypes = parameters.Select(p => RewriteHelper.GetFullCecilName(p.ParameterType));
        return string.Join(",", paramTypes);
    }

    public sealed class MapMethodToStaticKeyValue(MethodInfo srcMethod, MethodInfo newMethod)
    {
        public readonly MethodInfo srcMethod = srcMethod;
        public readonly MethodInfo newMethod = newMethod;
        public string srcMethodFullName = GetMethodFullName(srcMethod);
        public string newMethodFullName = GetMethodFullName(newMethod);
        public void AddPramToSrc(Type newType)
        {
            bool isEmptyParam = this.srcMethodFullName[this.srcMethodFullName.Length - 2] == '(';
            if (isEmptyParam)
            {
                this.srcMethodFullName = this.srcMethodFullName.Replace(")", $"{newType.FullName})");
            }
            else
            {
                this.srcMethodFullName = this.srcMethodFullName.Replace(")", $",{newType.FullName})");
            }
        }
    }
    //key == Src Method FullName
    public readonly Dictionary<string, MapMethodToStaticKeyValue> MapMethods = new();

    //Key: method.Name;
    public readonly Dictionary<string, MapMethodWithCallback> MapMethodWithCallbacks = new();

    public MapMethodToStaticMethodRewriter() : base("Rewrite map method to static method")
    {

    }

    public static string GetMethodFullName(MethodInfo method)
    {
        var type = method.DeclaringType;
        string returnFullName = FormatCecilType(method.ReturnType);
        string paramsFullName = FormatCecilParameterList(method.GetParameters());
        return $"{returnFullName} {type}::{method.Name}({paramsFullName})";
    }
    public delegate bool SelectMethodCallbackDelegate(MethodInfo method);
    public delegate void PostEditAddMethod(MapMethodToStaticKeyValue mapMethod);

    public MethodInfo HandleSelectorMethod(Type type, SelectMethodCallbackDelegate selectorCallback)
    {
        var methods = type.GetMethods();
        foreach (var m in methods)
        {
            if (selectorCallback.Invoke(m))
                return m;
        }
        return null;
    }

    public MapMethodToStaticMethodRewriter Add(Type srcType,
        SelectMethodCallbackDelegate srcMethodSelector,
        Type newType,
        SelectMethodCallbackDelegate newMethodSelector,
        PostEditAddMethod option = null)
    {

        var srcMethod = this.HandleSelectorMethod(srcType, srcMethodSelector);
        Console.WriteLine("MapMethodToStatic src: " + srcMethod);

        if (srcMethod == null)
        {
            Console.WriteLine("Error not found src method in type: " + srcType);
            return this;
        }

        var newMethod = this.HandleSelectorMethod(newType, newMethodSelector);
        if (newMethod == null)
        {
            Console.WriteLine("Error not found new method in type: " + newType);
            return this;
        }

        var mapMethod = new MapMethodToStaticKeyValue(srcMethod, newMethod);

        if (option != null)
            option.Invoke(mapMethod);

        //added make sure you finsih edit src & new method full name
        this.MapMethods.TryAdd(mapMethod.srcMethodFullName, mapMethod);
        return this;
    }
    public override bool Handle(ModuleDefinition module, ILProcessor cil, Instruction instruction)
    {
        var thisMethod = RewriteHelper.AsMethodReference(instruction);
        if (thisMethod == null)
            return false;

        string fullName = thisMethod.FullName;
        string fullNameStartWith = fullName.Substring(0, fullName.IndexOf("::" + thisMethod.Name) + thisMethod.Name.Length + 2);
        if (this.MapMethodWithCallbacks.TryGetValue(fullNameStartWith, out var mapMethodStartWith))
        {
            bool isMark = mapMethodStartWith.callback(mapMethodStartWith, thisMethod, module, cil, instruction);
            if (isMark)
                return this.MarkRewritten();
        }

        if (this.MapMethods.TryGetValue(thisMethod.FullName, out var newMethodRef))
        {
            instruction.Operand = module.ImportReference(newMethodRef.newMethod);
            return this.MarkRewritten();
        }
        return false;
    }

    public MapMethodToStaticMethodRewriter AddWithFullNameMaching(
        string startWith,
        MapMethodWithCallback.CallbackDelegate callback)
    {

        var mapMethod = new MapMethodWithCallback(startWith, callback);
        this.MapMethodWithCallbacks.TryAdd(startWith, mapMethod);

        return this;
    }
}
