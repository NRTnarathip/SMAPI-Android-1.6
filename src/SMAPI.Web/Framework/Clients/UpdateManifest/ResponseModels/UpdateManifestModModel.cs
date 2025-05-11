namespace StardewModdingAPI.Web.Framework.Clients.UpdateManifest.ResponseModels;

/// <summary>The data model for a mod in an update manifest file.</summary>
internal class UpdateManifestModModel
{
    /*********
    ** Accessors
    *********/
    /// <summary>The mod's name.</summary>
    public string? Name { get; }

    /// <summary>The mod page URL from which to download updates.</summary>
    public string? ModPageUrl { get; }

    /// <summary>The available versions for this mod.</summary>
    public UpdateManifestVersionModel[] Versions { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="name">The mod's name.</param>
    /// <param name="modPageUrl">The mod page URL from which to download updates.</param>
    /// <param name="versions">The available versions for this mod.</param>
    public UpdateManifestModModel(string? name, string? modPageUrl, UpdateManifestVersionModel[]? versions)
    {
        this.Name = name;
        this.ModPageUrl = modPageUrl;
        this.Versions = versions ?? [];
    }
}
