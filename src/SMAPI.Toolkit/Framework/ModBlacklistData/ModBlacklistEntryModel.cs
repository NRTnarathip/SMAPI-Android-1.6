namespace StardewModdingAPI.Toolkit.Framework.ModBlacklistData;

/// <summary>A mod entry in the <see cref="ModBlacklistModel"/>.</summary>
public class ModBlacklistEntryModel
{
    /// <summary>The manifest IDs to block (if any).</summary>
    public string? Id { get; }

    /// <summary>The MD5 hashes of the entry DLL to block (if any).</summary>
    /// <remarks>Due to the chance of MD5 collisions, this should only be used in addition to the <see cref="Id"/>.</remarks>
    public string? EntryDllHash { get; }

    /// <summary>A player-friendly explanation of why the mod is blocked and what they should do next.</summary>
    public string Message { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="id"><inheritdoc cref="Id" path="/summary"/></param>
    /// <param name="entryDllHash"><inheritdoc cref="EntryDllHash" path="/summary"/></param>
    /// <param name="message"><inheritdoc cref="Message" path="/summary"/></param>
    public ModBlacklistEntryModel(string? id, string? entryDllHash, string message)
    {
        this.Id = id;
        this.EntryDllHash = entryDllHash;
        this.Message = message;
    }
}
