using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewModdingAPI.Web.ViewModels;

/// <summary>Metadata for the mod list page.</summary>
public class ModListModel
{
    /*********
    ** Accessors
    *********/
    /// <summary>The URI from which to fetch the mod data.</summary>
    public string? FetchUri { get; }

    /// <summary>The mods to display instead of the <see cref="FetchUri"/>, if set.</summary>
    public ModModel[]? Mods { get; }

    /// <summary>When the data was last updated.</summary>
    public DateTimeOffset LastUpdated { get; }

    /// <summary>Whether the data hasn't been updated in a while.</summary>
    public bool IsStale { get; }

    /// <summary>Whether the mod metadata is available.</summary>
    public bool HasData => this.FetchUri != null || this.Mods?.Length > 0;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="fetchUri">The URI from which to fetch the mod data.</param>
    /// <param name="mods">The mods to display instead of the <paramref name="fetchUri"/>, if set.</param>
    /// <param name="lastUpdated">When the data was last updated.</param>
    /// <param name="isStale">Whether the data hasn't been updated in a while.</param>
    public ModListModel(string? fetchUri, IEnumerable<ModModel>? mods, DateTimeOffset lastUpdated, bool isStale)
    {
        this.FetchUri = fetchUri;
        this.Mods = mods?.ToArray();
        this.LastUpdated = lastUpdated;
        this.IsStale = isStale;
    }
}
