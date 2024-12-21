using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI.Framework.ModLoading.Framework;
using StardewValley;

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6;

/// <summary>Maps Stardew Valley 1.6.14's <see cref="Stats"/> methods to their newer form to avoid breaking older mods.</summary>
/// <remarks>This is public to support SMAPI rewriting and should never be referenced directly by mods. See remarks on <see cref="ReplaceReferencesRewriter"/> for more info.</remarks>
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named due to technical limitations, since we can't have two different facades for the same fields in 1.6.0 and 1.6.15.")]
public class StatsFacade_1615 : Stats, IRewriteFacade
{
    /*********
    ** Accessors
    *********/
    public new SerializableDictionaryWithCaseInsensitiveKeys<int> specificMonstersKilled
    {
        get => base.specificMonstersKilled;
    }

    public SerializableDictionaryWithCaseInsensitiveKeys<uint> Values
    {
        get => base.Values;
    }


    /*********
    ** Private methods
    *********/
    private StatsFacade_1615()
    {
        RewriteHelper.ThrowFakeConstructorCalled();
    }
}
