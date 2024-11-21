using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Framework;
using StardewValley.Menus;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.Mods;
using StardewModdingAPI.Framework.Events;
using System.Diagnostics;

namespace StardewModdingAPI.Mobile;

[HarmonyPatch]
internal static class VersionInfoMenu
{
    internal static void Setup()
    {
        SCore.OnRenderedStepEvent += SCore_OnRenderedStepEvent;
    }

    static void SCore_OnRenderedStepEvent(RenderSteps step, SpriteBatch spriteBatch, RenderTarget2D? renderTarget)
    {
        if (step == RenderSteps.Menu)
        {
            RenderVerionInfo(spriteBatch);
        }
    }

    static SpriteFont font;
    private static void RenderVerionInfo(SpriteBatch spriteBatch)
    {
        //check if it's on customize character or other sub menu, so we don't should render
        var titleMenu = Game1.activeClickableMenu as TitleMenu;
        if (titleMenu == null || TitleMenu.subMenu != null)
            return;

        var viewport = Game1.viewport;
        float centerX = viewport.Width / 2f;
        if (font == null && Game1.smallFont != null)
            font = Game1.smallFont;

        if (font == null)
            return;

        string text = $"SMAPI Version: {Constants.ApiVersion}";
        var textSizeRect = font.MeasureString(text);
        var pos = Vector2.Zero;
        pos.X = centerX - (textSizeRect.X / 2f);
        pos.Y = 16;

        spriteBatch.DrawString(font, text, pos, Color.White);
        spriteBatch.DrawString(font, "Port By NRTnarathip", pos + new Vector2(0, textSizeRect.Y), Color.White);
    }
}
