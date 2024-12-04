using System;
using System.Diagnostics;
using Android.AdServices.Topics;
using HarmonyLib;
using Iced.Intel;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Mobile;

namespace StardewModdingAPI.Mobile.Facade;

[HarmonyPatch]
static class MenuWithInventoryPatcher
{
    [HarmonyPatch(methodType: MethodType.Constructor)]
    [HarmonyPatch(typeof(MenuWithInventory))]
    [HarmonyPatch(new Type[] {
        typeof(InventoryMenu.highlightThisItem), // Parameter 1: highlighterMethod
        typeof(bool), // Parameter 2: okButton
        typeof(bool), // Parameter 3: trashCan
        typeof(int),  // Parameter 4: xPositionOnScreen
        typeof(int),  // Parameter 5: yPositionOnScreen
        typeof(int),  // Parameter 6: width
        typeof(int)   // Parameter 7: height
    })]
    [HarmonyPostfix]
    static void FixCtor(MenuWithInventory __instance,
        InventoryMenu.highlightThisItem highlighterMethod = null,
        bool okButton = false, bool trashCan = false,
        int xPositionOnScreen = 0, int yPositionOnScreen = 0,
        int width = 1280, int height = 720)
    {

        var menu = __instance;
        int xPosOnScreen = menu.xPositionOnScreen;
        int yPosOnScreen = menu.yPositionOnScreen;

        if (trashCan)
        {
            __instance.trashCan = new ClickableTextureComponent(
                new Rectangle(xPosOnScreen + width + 4, yPosOnScreen + height - 192 - 32 - IClickableMenu.borderWidth - 104, 64, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f)
            {
                myID = 5948,
                downNeighborID = 4857,
                leftNeighborID = 12,
                upNeighborID = 106
            };
            Console.WriteLine("Done patch create trashCan Obj in side: " + __instance);
        }
    }

    //[HarmonyPrefix]
    //[HarmonyPatch(typeof(SoundsHelper), "PlayLocal")]
    //static void PrefixPlayLocal(string cueName, GameLocation location, Vector2? position, int? pitch, SoundContext context, ICue cue)
    //{
    //    Console.WriteLine("prefix play local: cueName: " + cueName);
    //}
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Game1), "playSound", [typeof(string), typeof(int)])]
    static void playSound(string cueName, int? pitch = null)
    {
        return;
        Console.WriteLine("prefix playSound: cueName: " + cueName);
        var stack = new StackTrace();
        foreach (var f in stack.GetFrames())
        {
            var method = f.GetMethod();
            Console.WriteLine("f: " + method.Name + ", in class: " + method.DeclaringType);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemGrabMenu), "receiveLeftClick")]
    static void receiveLeftClick(ItemGrabMenu __instance, int x, int y, bool playSound = true)
    {
        Console.WriteLine("prefix receiveLeftClick: ");
        Console.WriteLine($"x: {x}, y: {y}, playSound: {playSound}");
        var upperRightCloseButton = __instance.upperRightCloseButton;
        var bound = upperRightCloseButton.bounds;
        Console.WriteLine($"bounds: width: {bound.Width}, height: {bound.Height}, size: {bound.Size}");
        Console.WriteLine($"bound: x: {bound.X}, y: {bound.Y}, center: {bound.Center}");

        if (upperRightCloseButton != null && upperRightCloseButton.containsPoint(x, y))
        {
            //OnTapUpperRightCloseButton();
            Console.WriteLine("try close menu OnTapUpperRightCloseButton()");
        }
        var topInv = (Rectangle)AccessTools.Field(typeof(ItemGrabMenu), "topInv").GetValue(__instance);
        var bottomInv = (Rectangle)AccessTools.Field(typeof(ItemGrabMenu), "bottomInv").GetValue(__instance);
        Console.WriteLine("topInv: " + topInv);
        Console.WriteLine("bottomInv: " + bottomInv);

        if (y < topInv.Y || (x < topInv.X && y < bottomInv.Y) || (x > topInv.Right && y < bottomInv.Y))
        {
            //OnTapUpperRightCloseButton();
            Console.WriteLine("try close menu 2 at OnTapUpperRightCloseButton()");
        }
    }


}
