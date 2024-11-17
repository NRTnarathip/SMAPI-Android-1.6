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

namespace StardewModdingAPI.Android;

[HarmonyPatch]
internal static class VersionInfoMenu
{
    internal static void Init()
    {
        SCore.EventOnRendered += OnRendered;
    }

    static void OnRendered(RenderTarget2D renderTarget)
    {
        var spriteBatch = Game1.spriteBatch;
        bool wasOpen = spriteBatch.IsOpen(SCore.Instance.GetReflector);
        bool hadRenderTarget = Game1.graphics.GraphicsDevice.RenderTargetCount > 0;

        if (!hadRenderTarget && !Game1.IsOnMainThread())
            return; // can't set render target on background thread

        try
        {
            if (!wasOpen)
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            if (!hadRenderTarget)
            {
                renderTarget ??= Game1.game1.uiScreen?.IsDisposed != true
                    ? Game1.game1.uiScreen
                    : Game1.nonUIRenderTarget;

                if (renderTarget != null)
                    Game1.SetRenderTarget(renderTarget);
            }
            RenderVerionInfo(spriteBatch, renderTarget);
        }
        finally
        {
            if (!wasOpen)
                spriteBatch.End();

            if (!hadRenderTarget && renderTarget != null)
                Game1.SetRenderTarget(null);
        }
    }

    static SpriteFont font;
    private static void RenderVerionInfo(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
    {
        //check if it's on customize character or other sub menu, so we don't should render
        if (TitleMenu.subMenu != null)
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
