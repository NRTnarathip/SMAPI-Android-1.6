using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Design;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StardewModdingAPI.Mobile.Vectors;

internal static class JsonPatcher
{
    public static void ApplyPatch(Harmony hp)
    {
        return;

        Console.WriteLine("try patch json converter");
        var ToObject_MethodInfo = AccessTools.Method(
            typeof(JToken),
            nameof(JToken.ToObject),
            [typeof(Type)]);
        hp.Patch(
            original: ToObject_MethodInfo,
            prefix: new(typeof(JsonPatcher), nameof(ToObject))
        );
    }

    static void ToObject(ref object __result, JToken __instance, Type objectType)
    {
        var token = __instance;
        Console.WriteLine($"Prefix ToObject(objectType: {objectType}); " +
            $"\n    token: '{token}'" +
            $"\n    type: {token.Type}");
    }

    [HarmonyPatch(typeof(JsonToken), nameof(ToObject), [typeof(Type), typeof(JsonSerializer)])]
    static void ToObject(Type? objectType, JsonSerializer jsonSerializer)
    {
        Console.WriteLine($"Prefix  ToObject(objectType: {objectType}, jsonSerializer)");
    }
}

