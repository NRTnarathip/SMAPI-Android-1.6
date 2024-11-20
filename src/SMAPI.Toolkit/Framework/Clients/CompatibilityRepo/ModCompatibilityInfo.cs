namespace StardewModdingAPI.Toolkit.Framework.Clients.CompatibilityRepo;

/// <summary>Compatibility info for a mod.</summary>
public class ModCompatibilityInfo
{
    /*********
    ** Accessors
    *********/
    /// <summary>The compatibility status.</summary>
    public ModCompatibilityStatus Status { get; }

    /// <summary>The human-readable summary of the compatibility status or workaround, in Markdown format.</summary>
    public string? Summary { get; }

    /// <summary>An HTML version of <see cref="Summary"/>, if different.</summary>
    public string? HtmlSummary { get; }

    /// <summary>The game or SMAPI version which broke this mod, if applicable.</summary>
    public string? BrokeIn { get; }

    /// <summary>The version of the latest unofficial update, if applicable.</summary>
    public ISemanticVersion? UnofficialVersion { get; }

    /// <summary>The URL to the latest unofficial update, if applicable.</summary>
    public string? UnofficialUrl { get; }


    /*********
    ** Accessors
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="status">The compatibility status.</param>
    /// <param name="summary">The human-readable summary of the compatibility status or workaround, in Markdown format.</param>
    /// <param name="htmlSummary">An HTML version of <paramref name="summary"/>, if different.</param>
    /// <param name="brokeIn">The game or SMAPI version which broke this mod, if applicable.</param>
    /// <param name="unofficialVersion">The version of the latest unofficial update, if applicable.</param>
    /// <param name="unofficialUrl">The URL to the latest unofficial update, if applicable.</param>
    public ModCompatibilityInfo(ModCompatibilityStatus status, string? summary, string? htmlSummary, string? brokeIn, ISemanticVersion? unofficialVersion, string? unofficialUrl)
    {
        this.Status = status;
        this.Summary = summary;
        this.HtmlSummary = htmlSummary;
        this.BrokeIn = brokeIn;
        this.UnofficialVersion = unofficialVersion;
        this.UnofficialUrl = unofficialUrl;
    }
}
