using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Framework.ModLoading.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Pathfinding;

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6;

/// <summary>Maps Stardew Valley 1.5.6's <see cref="NPC"/> methods to their newer form to avoid breaking older mods.</summary>
/// <remarks>This is public to support SMAPI rewriting and should never be referenced directly by mods. See remarks on <see cref="ReplaceReferencesRewriter"/> for more info.</remarks>
public abstract class NpcFacade : NPC, IRewriteFacade
{
    /*********
    ** Accessors
    *********/
    public bool eventActor
    {
        get => base.EventActor;
        set => base.EventActor = value;
    }

    public new int Gender
    {
        get => (int)base.Gender;
        set => base.Gender = (Gender)value;
    }


    /*********
    ** Public methods
    *********/
    public static NPC Constructor(AnimatedSprite sprite, Vector2 position, string defaultMap, int facingDirection, string name, bool datable, Dictionary<int, int[]> schedule, Texture2D portrait)
    {
        return new NPC(sprite, position, defaultMap, facingDirection, name, datable, portrait);
    }

    public static NPC Constructor(AnimatedSprite sprite, Vector2 position, string defaultMap, int facingDir, string name, Dictionary<int, int[]> schedule, Texture2D portrait, bool eventActor, string? syncedPortraitPath = null)
    {
        NPC npc = new(sprite, position, defaultMap, facingDir, name, portrait, eventActor);

        if (!string.IsNullOrWhiteSpace(syncedPortraitPath))
        {
            npc.Portrait = Game1.content.Load<Texture2D>(syncedPortraitPath);
            npc.portraitOverridden = true;
        }

        return npc;
    }

    public bool isBirthday(string season, int day)
    {
        // call new method if possible
        if (season == Game1.currentSeason && day == Game1.dayOfMonth)
            return base.isBirthday();

        // else replicate old behavior
        return
            base.Birthday_Season != null
            && base.Birthday_Season == season
            && base.Birthday_Day == day;
    }

    public static void populateRoutesFromLocationToLocationList()
    {
        WarpPathfindingCache.PopulateCache();
    }

    public void showTextAboveHead(string Text, int spriteTextColor = -1, int style = NPC.textStyle_none, int duration = 3000, int preTimer = 0)
    {
        Color? color = spriteTextColor != -1 ? SpriteText.getColorFromIndex(spriteTextColor) : null;
        base.showTextAboveHead(Text, color, style, duration, preTimer);
    }


    /*********
    ** Private methods
    *********/
    private NpcFacade()
    {
        RewriteHelper.ThrowFakeConstructorCalled();
    }
}
