using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI.Toolkit.Framework.Clients.CompatibilityRepo;

namespace StardewModdingAPI.Web.Framework.Caching.CompatibilityRepo;

/// <summary>Manages cached compatibility list data.</summary>
internal interface ICompatibilityCacheRepository : ICacheRepository
{
    /*********
    ** Methods
    *********/
    /// <summary>Get the cache metadata.</summary>
    /// <param name="metadata">The fetched metadata.</param>
    bool TryGetCacheMetadata([NotNullWhen(true)] out Cached<CompatibilityListMetadata>? metadata);

    /// <summary>Get the cached compatibility list.</summary>
    /// <param name="filter">A filter to apply, if any.</param>
    IEnumerable<Cached<ModCompatibilityEntry>> GetMods(Func<ModCompatibilityEntry, bool>? filter = null);

    /// <summary>Save data fetched from the compatibility list.</summary>
    /// <param name="mods">The mod data.</param>
    void SaveData(IEnumerable<ModCompatibilityEntry> mods);
}
