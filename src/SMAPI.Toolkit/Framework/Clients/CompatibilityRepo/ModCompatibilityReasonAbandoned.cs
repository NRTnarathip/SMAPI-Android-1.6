namespace StardewModdingAPI.Toolkit.Framework.Clients.CompatibilityRepo;

/// <summary>The reason a mod is marked abandoned.</summary>
public enum ModCompatibilityReasonAbandoned
{
    /// <summary>Not applicable.</summary>
    None,

    /// <summary>The mod has been explicitly and definitively abandoned by the author.</summary>
    Author,

    /// <summary>The mod pages or files have been deleted, so the mod is no longer available.</summary>
    Deleted,

    /// <summary>The mod pages have been hidden, so the mod is no longer available.</summary>
    Hidden
}
