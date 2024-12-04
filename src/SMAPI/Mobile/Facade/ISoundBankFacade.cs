using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI.Framework.ModLoading.Rewriters;
using StardewValley;

namespace StardewModdingAPI.Mobile.Facade;

internal interface ISoundBankFacade : ISoundBank, IRewriteFacade
{
    /// <summary>Add a sound to the sound bank.</summary>
    /// <param name="definition">The sound definition to add.</param>
    void AddCue(CueDefinition definition);
}
