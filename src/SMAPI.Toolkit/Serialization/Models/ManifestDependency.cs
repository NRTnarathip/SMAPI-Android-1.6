using Newtonsoft.Json;

namespace StardewModdingAPI.Toolkit.Serialization.Models;

/// <summary>A mod dependency listed in a mod manifest.</summary>
public class ManifestDependency : IManifestDependency
{
    /*********
    ** Accessors
    *********/
    /// <summary>The unique mod ID to require.</summary>
    public string UniqueID { get; }

    /// <summary>The minimum required version (if any).</summary>
    public ISemanticVersion? MinimumVersion { get; }

    /// <summary>Whether the dependency must be installed to use the mod.</summary>
    public bool IsRequired { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="uniqueId">The unique mod ID to require.</param>
    /// <param name="minimumVersion">The minimum required version (if any).</param>
    /// <param name="required">Whether the dependency must be installed to use the mod.</param>
    public ManifestDependency(string uniqueId, string? minimumVersion, bool required = true)
        : this(
            uniqueId: uniqueId,
            minimumVersion: !string.IsNullOrWhiteSpace(minimumVersion)
                ? new SemanticVersion(minimumVersion)
                : null,
            required: required
        )
    { }

    /// <summary>Construct an instance.</summary>
    /// <param name="uniqueId">The unique mod ID to require.</param>
    /// <param name="minimumVersion">The minimum required version (if any).</param>
    /// <param name="required">Whether the dependency must be installed to use the mod.</param>
    [JsonConstructor]
    public ManifestDependency(string uniqueId, ISemanticVersion? minimumVersion, bool required = true)
    {
        this.UniqueID = Manifest.NormalizeWhitespace(uniqueId);
        this.MinimumVersion = minimumVersion;
        this.IsRequired = required;
    }
}
