using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace StardewModdingAPI.Toolkit.Framework.Clients.ModDropExport.ResponseModels;

/// <summary>The metadata for a mod from the ModDrop export API.</summary>
public class ModDropModExport
{
    /// <summary>The mod ID.</summary>
    public uint Id { get; set; }

    /// <summary>The mod page title.</summary>
    public string? Title { get; set; }

    /// <summary>The author of the mod.</summary>
    public string? AuthorName { get; set; }

    /// <summary>The name of the user who uploaded the mod.</summary>
    public string? UserName { get; set; }

    /// <summary>When the mod was published, as a Unix millisecond timestamp since epoch.</summary>
    public long DatePublished { get; set; }

    /// <summary>When the mod was published, as a Unix millisecond timestamp since epoch.</summary>
    public long DateUpdated { get; set; }

    /// <summary>Whether the mod page is deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Whether the mod page is published.</summary>
    public bool IsPublished { get; set; }

    /// <summary>Whether the mod page is unlocked.</summary>
    public bool IsUnlocked { get; set; }

    /// <summary>The mod page URL.</summary>
    public string? PageUrl { get; set; }

    /// <summary>???</summary>
    public int Status { get; set; }

    /// <summary>The files uploaded for the mod.</summary>
    public ModDropFileExport[] Files { get; set; } = [];

    /// <summary>The extra fields returned by the export API, if any.</summary>
    [JsonExtensionData]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used to track any new data provided by the API.")]
    public Dictionary<string, object>? OtherFields;
}
