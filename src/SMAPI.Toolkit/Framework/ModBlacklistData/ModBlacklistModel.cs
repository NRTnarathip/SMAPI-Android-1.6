namespace StardewModdingAPI.Toolkit.Framework.ModBlacklistData;

/// <summary>A list of malicious mods which should be blocked by SMAPI.</summary>
public class ModBlacklistModel
{
    /*********
    ** Accessors
    *********/
    /// <summary>Metadata about malicious or harmful SMAPI mods which are disabled by default.</summary>
    public ModBlacklistEntryModel[] Blacklist { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="blacklist"><inheritdoc cref="Blacklist" path="/summary"/></param>
    public ModBlacklistModel(ModBlacklistEntryModel[]? blacklist)
    {
        this.Blacklist = blacklist ?? [];
    }
}
