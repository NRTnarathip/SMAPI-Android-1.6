using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StardewModdingAPI.Toolkit.Framework.UpdateData;

namespace StardewModdingAPI.Web.Framework.Metrics;

/// <summary>The metrics for a specific site.</summary>
internal class MetricsModel
{
    /*********
    ** Accessors
    *********/
    /// <summary>The number of times an update key returned data from the cache.</summary>
    public int CacheHits { get; private set; }

    /// <summary>The number of times an update key successfully fetched data from the remote mod site.</summary>
    public int SuccessCacheMisses { get; private set; }

    /// <summary>The number of times an update key could not fetch data from the remote mod site (e.g. mod page didn't exist or mod site returned an API error).</summary>
    public int ErrorCacheMisses { get; private set; }

    /// <summary>The unique mod IDs requested from each site.</summary>
    [JsonIgnore]
    public HashSet<string?> UniqueKeys { get; } = [];

    /// <summary>The number of unique mod IDs requested from each site.</summary>
    public int UniqueModsChecked => this.UniqueKeys.Count;


    /*********
    ** Public methods
    *********/
    /// <summary>Track the update-check result for a specific update key.</summary>
    /// <param name="updateKey">The update key that was requested.</param>
    /// <param name="wasCached">Whether the data was returned from the cache; else it was fetched from the remote modding site.</param>
    /// <param name="wasSuccessful">Whether the data was fetched successfully from the remote modding site.</param>
    public void TrackUpdateKey(UpdateKey updateKey, bool wasCached, bool wasSuccessful)
    {
        // normalize site key
        ModSiteKey site = updateKey.Site;
        if (!Enum.IsDefined(site))
            site = ModSiteKey.Unknown;

        // update metrics
        if (wasCached)
            this.CacheHits++;
        else if (wasSuccessful)
            this.SuccessCacheMisses++;
        else
            this.ErrorCacheMisses++;

        this.UniqueKeys.Add(updateKey.ID?.Trim());
    }

    /// <summary>Merge the values from another metrics model into this one.</summary>
    /// <param name="other">The metrics to merge into this model.</param>
    public void AggregateFrom(MetricsModel other)
    {
        this.CacheHits += other.CacheHits;
        this.SuccessCacheMisses += other.SuccessCacheMisses;
        this.ErrorCacheMisses += other.ErrorCacheMisses;

        foreach (string? id in other.UniqueKeys)
            this.UniqueKeys.Add(id);
    }
}
