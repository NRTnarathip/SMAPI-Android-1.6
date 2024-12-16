using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace StardewModdingAPI.Mobile;

[HarmonyPatch]
internal static class AndroidCreateSaveMenuDebug
{
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

    static void Postfix_MobileCustomizer(MobileCustomizer __instance,
        int x, int y, int width, int height,
        CharacterCustomization.Source source,
        Clothing item)
    {
        Console.WriteLine("Postfix_MobileCustomizer");
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
