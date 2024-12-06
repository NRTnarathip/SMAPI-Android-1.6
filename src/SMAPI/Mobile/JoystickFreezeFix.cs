using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using static Android.Graphics.BitmapFactory;

namespace StardewModdingAPI.Mobile;

[HarmonyPatch]
internal static class JoystickFreezeFix
{
    public static void Init()
    {
        var hp = AndroidPatcher.harmony;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Game1), "_update")]
    static void _update(GameTime gameTime)
    {
        Game1 game1 = Game1.game1;

        bool isClamp = Game1.activeClickableMenu?.shouldClampGamePadCursor() ?? false;
        //if (Game1.options.gamepadControls && isClamp)
        {
            Point mousePositionRaw = Game1.getMousePositionRaw();
            Console.WriteLine("current pos mouse raw: " + mousePositionRaw);
            Rectangle localWindow = new(0, 0, game1.localMultiplayerWindow.Width, game1.localMultiplayerWindow.Height);
            Console.WriteLine("local window: " + localWindow.Size);

            if (mousePositionRaw.X < localWindow.X)
            {
                mousePositionRaw.X = localWindow.X;
                Console.WriteLine("clamp x at window X: " + localWindow.X);
            }
            else if (mousePositionRaw.X > localWindow.Right)
            {
                mousePositionRaw.X = localWindow.Right;
                Console.WriteLine("clamp x at window Right: " + localWindow.X);
            }
            if (mousePositionRaw.Y < localWindow.Y)
            {
                mousePositionRaw.Y = localWindow.Y;
                Console.WriteLine("clamp y window Y: " + localWindow.Y);
            }
            else if (mousePositionRaw.Y > localWindow.Bottom)
            {
                mousePositionRaw.Y = localWindow.Bottom;
                Console.WriteLine("clamp y at bottom: " + localWindow.Y);
            }
            //setMousePositionRaw(mousePositionRaw.X, mousePositionRaw.Y);
            Console.WriteLine("try set new mouse: " + mousePositionRaw);
        }
    }

}
