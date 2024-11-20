using System;
using StardewModdingAPI.Framework.ModLoading.Framework;
using StardewValley;
using StardewValley.Internal;

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6;

/// <summary>Maps Stardew Valley 1.6's <see cref="ItemQueryContext"/> methods to their newer form in 1.6.14 to avoid breaking older mods.</summary>
/// <remarks>This is public to support SMAPI rewriting and should never be referenced directly by mods. See remarks on <see cref="ReplaceReferencesRewriter"/> for more info.</remarks>
public class ItemQueryContextFacade : ItemQueryContext, IRewriteFacade
{
    /*********
    ** Public methods
    *********/
    public static ItemQueryContext Constructor(ItemQueryContext parentContext)
    {
        return new ItemQueryContext(parentContext, null);
    }

    public static ItemQueryContext Constructor(GameLocation location, Farmer player, Random random)
    {
        return new ItemQueryContext(location, player, random, null);
    }


    /*********
    ** Private methods
    *********/
    private ItemQueryContextFacade()
    {
        RewriteHelper.ThrowFakeConstructorCalled();
    }
}
