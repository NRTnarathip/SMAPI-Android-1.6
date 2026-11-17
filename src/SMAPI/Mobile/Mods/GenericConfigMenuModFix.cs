using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework;
using StardewValley;
using StardewValley.Menus;

namespace StardewModdingAPI.Mobile.Mods;

internal static class GenericConfigMenuModFix
{
    static IMonitor monitor;
    internal static void Init(AndroidModFixManager modFix)
    {
        monitor = SCore.Instance.SMAPIMonitor;
        modFix.RegisterOnModLoaded("GenericModConfigMenu.dll", OnModLoaded);
    }
    static MethodInfo ModEntry_OnRendered_MethodInfo;
    static FieldInfo ModEntry_ConfigButton_FieldInfo;
    static PropertyInfo Button_Bounds_PropertyInfo;
    static MethodInfo TitleMenu_ShouldDrawCursor_MethodInfo;
    const int GamepadButtonId = 509800; // GMCM's title button
    static void OnModLoaded(Assembly asm)
    {
        monitor.Log("Start GenericModConfigFix");

        var hp = AndroidPatcher.harmony;
        var modEntry = asm.GetType("GenericModConfigMenu.Mod");
        ModEntry_OnRendered_MethodInfo = AccessTools.Method(modEntry, "OnRendered");
        ModEntry_ConfigButton_FieldInfo = AccessTools.Field(modEntry, "ConfigButton");
        Button_Bounds_PropertyInfo = ModEntry_ConfigButton_FieldInfo?.FieldType.GetProperty("Bounds");
        TitleMenu_ShouldDrawCursor_MethodInfo = AccessTools.Method(typeof(TitleMenu), "ShouldDrawCursor");
        hp.Patch(
            original: ModEntry_OnRendered_MethodInfo,
            prefix: new(typeof(GenericConfigMenuModFix), nameof(ModEntry_OnRendered))
        );
        //new rendered on Overlays
        SCore.OnRenderedStepEvent += SCore_OnRenderedStepEvent;

        monitor.Log("Patch ModEntry.OnRendered()");

        monitor.Log("Done GenericModConfigFix");
    }

    private static void SCore_OnRenderedStepEvent(StardewValley.Mods.RenderSteps step, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Microsoft.Xna.Framework.Graphics.RenderTarget2D? renderTarget)
    {
        if (SModHooks.CurrentRenderedStep == StardewValley.Mods.RenderSteps.Overlays)
        {
            IModMetadata modAPI = SCore.Instance.GetModRegistry().Get("spacechase0.GenericModConfigMenu");

            // GMCM lays out and hit-tests in zoom units, this step draws in UI units
            float scale = Game1.options.zoomLevel / Game1.options.uiScale;
            float cursorAlpha = Game1.mouseCursorTransparency;
            Game1.mouseCursorTransparency = 0; // GMCM's button draws the cursor too, which would be scaled
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(scale));
            try
            {
                ModEntry_OnRendered_MethodInfo.Invoke(modAPI.Mod, [null, new RenderedEventArgs()]);
            }
            finally
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
                Game1.mouseCursorTransparency = cursorAlpha;
            }

            SyncGamepadButton(modAPI.Mod, scale);
            DrawCursorOverButton(spriteBatch);
        }
    }

    // title menu draws its cursor before this step, draw it again on top of the button
    static void DrawCursorOverButton(SpriteBatch spriteBatch)
    {
        if (Game1.activeClickableMenu is TitleMenu titleMenu && TitleMenu.subMenu == null && TitleMenu_ShouldDrawCursor_MethodInfo?.Invoke(titleMenu, null) is true)
            titleMenu.drawMouse(spriteBatch);
    }

    // GMCM sets the snap target in zoom units, title menu snaps in UI units
    static void SyncGamepadButton(object gmcm, float scale)
    {
        if (Game1.activeClickableMenu is not TitleMenu titleMenu || Button_Bounds_PropertyInfo == null)
            return;

        var button = ModEntry_ConfigButton_FieldInfo.GetValue(gmcm);
        var target = titleMenu.allClickableComponents?.Find(c => c?.myID == GamepadButtonId);
        if (button == null || target == null)
            return;

        var bounds = (Rectangle)Button_Bounds_PropertyInfo.GetValue(button);
        target.bounds = new Rectangle((int)(bounds.X * scale), (int)(bounds.Y * scale), (int)(bounds.Width * scale), (int)(bounds.Height * scale));
    }

    static bool ModEntry_OnRendered(object sender, RenderedEventArgs e)
    {
        if (SModHooks.CurrentRenderedStep != StardewValley.Mods.RenderSteps.Overlays)
            return false;

        return true;
    }
}
