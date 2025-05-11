using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace StardewModdingAPI.Toolkit.Framework.Clients.CurseForgeExport.ResponseModels;

/// <summary>The metadata for a mod from the CurseForge export API.</summary>
public class CurseForgeModExport
{
    /// <summary>The mod ID.</summary>
    public long Id { get; set; }

    /// <summary>The mod's display name.</summary>
    public string? Name { get; set; }

    /// <summary>The URL to the mod's web page on CurseForge.</summary>
    public string? ModPageUrl { get; set; }

    /// <summary>The authors of the mod.</summary>
    public CurseForgeAuthorExport[] Authors { get; set; } = [];

    /// <summary>When the mod was created.</summary>
    public DateTimeOffset DateCreated { get; set; }

    /// <summary>When the mod became public.</summary>
    public DateTimeOffset DateReleased { get; set; }

    /// <summary>When the mod was last modified.</summary>
    public DateTimeOffset DateModified { get; set; }

    /// <summary>The files uploaded for the mod.</summary>
    public CurseForgeFileExport[] Files { get; set; } = [];

    /// <summary>The extra fields returned by the export API, if any.</summary>
    [JsonExtensionData]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used to track any new data provided by the API.")]
    public Dictionary<string, object>? OtherFields;
}
