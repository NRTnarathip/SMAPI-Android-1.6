using Microsoft.Xna.Framework;
using StardewModdingAPI.Framework.ModLoading.Framework;
using StardewValley;
using StardewValley.Internal;
using StardewValley.WorldMaps;

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6;

/// <summary>Maps Stardew Valley 1.'s <see cref="WorldMapManager"/> methods to their newer form in 1.6.14 to avoid breaking older mods.</summary>
/// <remarks>This is public to support SMAPI rewriting and should never be referenced directly by mods. See remarks on <see cref="ReplaceReferencesRewriter"/> for more info.</remarks>
public class WorldMapManagerFacade : IRewriteFacade
{
    /*********
    ** Public methods
    *********/
    /// <summary>Get the map position which contains a given location and tile coordinate, if any.</summary>
    /// <param name="location">The in-game location.</param>
    /// <param name="tile">The tile coordinate within the location.</param>
    public static MapAreaPosition? GetPositionData(GameLocation location, Point tile)
    {
        return WorldMapManager.GetPositionData(location, tile, null)?.Data;
    }

    /// <summary>Get the map position which contains a given location and tile coordinate, if any.</summary>
    /// <param name="location">The in-game location.</param>
    /// <param name="tile">The tile coordinate within the location.</param>
    /// <param name="log">The detailed log to update with the steps used to match the position, if set.</param>
    internal static MapAreaPosition? GetPositionData(GameLocation location, Point tile, LogBuilder log)
    {
        return WorldMapManager.GetPositionData(location, tile, log)?.Data;
    }


    /*********
    ** Private methods
    *********/
    private WorldMapManagerFacade()
    {
        RewriteHelper.ThrowFakeConstructorCalled();
    }
}
