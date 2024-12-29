using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace StardewModdingAPI.Mobile;

//crash on Wizard Appearance
//debug only
#if false
[HarmonyPatch]
internal static class MonoClassFromMonoTypeInternalTester
{
    public static void Init(Harmony hp)
    {
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MobileCustomizer), MethodType.Constructor,
    [
        typeof(int),
        typeof(int),
        typeof(int),
        typeof(int),
        typeof(CharacterCustomization.Source),
        typeof(Clothing),
    ])]

    //crash on Wizard Appearance
    static void Postfix_MobileCustomizer(MobileCustomizer __instance,
        int x, int y, int width, int height,
        CharacterCustomization.Source source,
        Clothing item)
    {
        Console.WriteLine("Postfix_MobileCustomizer");
        Console.WriteLine("try to make crash");
        var stack = new StackTrace();
        var frameCount = stack.FrameCount;
        Console.WriteLine("frame count: " + frameCount);
        for (int i = 0; i < frameCount; i++)
        {
            Console.WriteLine("");
            Console.WriteLine($"frame: {i}");
            var frame = stack.GetFrame(i);
            Console.WriteLine("done get frame");
            Console.WriteLine("try get method");
            var method = frame.GetMethod();
            Console.WriteLine("method type: " + method.GetType());
            Console.WriteLine($"method: name {method.Name}");
            Console.WriteLine($"  reflected: {method.ReflectedType}");
            Console.WriteLine($"  FullyQualifiedName: {method.Module.FullyQualifiedName}");
            Console.WriteLine($"  member type: {method.MemberType}");
            Console.WriteLine($"  DeclaringType type: {method.DeclaringType}");
            Console.WriteLine($"  method.ToString(): " + method.ToString());
        }
        Console.WriteLine("try call stack.ToString()");
        var stackString = stack.ToString();
        Console.WriteLine("done");
        Console.WriteLine(stackString);


        (AccessTools.Field(typeof(MobileCustomizer), "nameBox").GetValue(__instance) as TextBox).Text = GenerateRandomText(5);
        (AccessTools.Field(typeof(MobileCustomizer), "farmnameBox").GetValue(__instance) as TextBox).Text = GenerateRandomText(5);
        (AccessTools.Field(typeof(MobileCustomizer), "favThingBox").GetValue(__instance) as TextBox).Text = GenerateRandomText(5);

        AccessTools.Field(typeof(MobileCustomizer), "skipIntro").SetValue(__instance, true);
    }
    static string GenerateRandomText(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        Random random = new Random();

        char[] result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }

        return new string(result);
    }
}

#else
internal static class MonoClassFromMonoTypeInternalTester
{
    public static void Init(Harmony hp)
    {

    }
}
#endif
