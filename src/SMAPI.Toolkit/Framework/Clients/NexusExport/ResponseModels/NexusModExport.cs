using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace StardewModdingAPI.Toolkit.Framework.Clients.NexusExport.ResponseModels;

/// <summary>The metadata for a mod from the Nexus Mods export API.</summary>
public class NexusModExport
{
    /// <summary>The unique internal mod identifier (not the public mod ID).</summary>
    public long Uid { get; set; }

    /// <summary>The mod's display name.</summary>
    public string? Name { get; set; }

    /// <summary>The author display name set for the mod.</summary>
    public string? Author { get; set; }

    /// <summary>The username for the user who uploaded the mod.</summary>
    public string? Uploader { get; set; }

    /// <summary>The ID for the user who uploaded the mod.</summary>
    [JsonProperty("uploader_id")]
    public int UploaderId { get; set; }

    /// <summary>The mod's semantic version.</summary>
    public string? Version { get; set; }

    /// <summary>The category ID.</summary>
    [JsonProperty("category_id")]
    public int CategoryId { get; set; }

    /// <summary>Whether the mod is published by the author.</summary>
    public bool Published { get; set; }

    /// <summary>Whether the mod is hidden by moderators.</summary>
    public bool Moderated { get; set; }

    /// <summary>Whether the mod page is visible to users.</summary>
    [JsonProperty("allow_view")]
    public bool AllowView { get; set; }

    /// <summary>Whether the mod is marked as containing adult content.</summary>
    public bool Adult { get; set; }

    /// <summary>The raw file description, containing BBCode with HTML <c>&lt;&lt;br /&gt;</c> tags.</summary>
    public string Description { get; set; }

    /// <summary>The files uploaded for the mod.</summary>
    public Dictionary<uint, NexusFileExport> Files { get; set; } = new();

    /// <summary>The extra fields returned by the export API, if any.</summary>
    [JsonExtensionData]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used to track any new data provided by the API.")]
    public Dictionary<string, object>? OtherFields;
}
