namespace StardewModdingAPI.Toolkit.Framework.Clients.CompatibilityRepo.RawDataModels;

/// <summary>As part of <see cref="RawModEntry"/>, a data override to apply to the mod's manifest or remote mod page data.</summary>
public class RawModDataOverride
{
    /// <summary>The data type to override.</summary>
    public string? Type { get; set; }

    /// <summary>The specific value to replace with the `to` field. For a version number, this must match the exact formatting before the version is parsed.</summary>
    public string? From { get; set; }

    /// <summary>The value to use instead of the `from` value.</summary>
    public string? To { get; set; }
}
