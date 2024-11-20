using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StardewModdingAPI.Toolkit.Framework.Clients.CompatibilityRepo;

namespace StardewModdingAPI.Web.Framework.Caching.CompatibilityRepo;

/// <summary>Manages cached compatibility list data in-memory.</summary>
internal class CompatibilityCacheMemoryRepository : BaseCacheRepository, ICompatibilityCacheRepository
{
    /*********
    ** Fields
    *********/
    /// <summary>The saved compatibility list metadata.</summary>
    private Cached<CompatibilityListMetadata>? Metadata;

    /// <summary>The cached compatibility list.</summary>
    private Cached<ModCompatibilityEntry>[] Mods = [];


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public bool TryGetCacheMetadata([NotNullWhen(true)] out Cached<CompatibilityListMetadata>? metadata)
    {
        metadata = this.Metadata;
        return metadata != null;
    }

    /// <inheritdoc />
    public IEnumerable<Cached<ModCompatibilityEntry>> GetMods(Func<ModCompatibilityEntry, bool>? filter = null)
    {
        foreach (var mod in this.Mods)
        {
            if (filter == null || filter(mod.Data))
                yield return mod;
        }
    }

    /// <inheritdoc />
    public void SaveData(IEnumerable<ModCompatibilityEntry> mods)
    {
        this.Metadata = new Cached<CompatibilityListMetadata>(new CompatibilityListMetadata());
        this.Mods = mods.Select(mod => new Cached<ModCompatibilityEntry>(mod)).ToArray();
    }
}
