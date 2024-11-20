namespace StardewModdingAPI.Toolkit.Framework.Clients.CompatibilityRepo.RawDataModels;

/// <summary>The main data model for the raw compatibility data.</summary>
public class RawCompatibilityList
{
    /// <summary>The compatibility data for C# SMAPI mods.</summary>
    public RawModEntry[]? Mods { get; set; }

    /// <summary>The compatibility data for broken content packs only. This allows providing workarounds and unofficial updates for content packs. However, compatible content packs shouldn't be listed here.</summary>
    public RawModEntry[]? BrokenContentPacks { get; set; }
}
