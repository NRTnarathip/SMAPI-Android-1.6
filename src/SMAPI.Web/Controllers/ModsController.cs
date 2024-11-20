using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StardewModdingAPI.Web.Framework.Caching;
using StardewModdingAPI.Web.Framework.Caching.CompatibilityRepo;
using StardewModdingAPI.Web.Framework.ConfigModels;
using StardewModdingAPI.Web.ViewModels;

namespace StardewModdingAPI.Web.Controllers;

/// <summary>Provides user-friendly info about SMAPI mods.</summary>
internal class ModsController : Controller
{
    /*********
    ** Fields
    *********/
    /// <summary>The cache in which to store mod metadata.</summary>
    private readonly ICompatibilityCacheRepository Cache;

    /// <summary>The number of minutes before which compatibility list data should be considered old.</summary>
    private readonly int StaleMinutes;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="cache">The cache in which to store mod metadata.</param>
    /// <param name="configProvider">The config settings for mod update checks.</param>
    public ModsController(ICompatibilityCacheRepository cache, IOptions<ModCompatibilityListConfig> configProvider)
    {
        ModCompatibilityListConfig config = configProvider.Value;

        this.Cache = cache;
        this.StaleMinutes = config.StaleMinutes;
    }

    /// <summary>Display information for all mods.</summary>
    [HttpGet]
    [Route("mods")]
    public ViewResult Index()
    {
        return this.View("Index", this.FetchData());
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Asynchronously fetch mod metadata from the compatibility list.</summary>
    public ModListModel FetchData()
    {
        // fetch cached data
        if (!this.Cache.TryGetCacheMetadata(out Cached<CompatibilityListMetadata>? metadata))
            return new ModListModel(Array.Empty<ModModel>(), lastUpdated: DateTimeOffset.UtcNow, isStale: true);

        // build model
        return new ModListModel(
            mods: this.Cache
                .GetMods()
                .Select(mod => new ModModel(mod.Data))
                .OrderBy(p => Regex.Replace((p.Name ?? "").ToLower(), "[^a-z0-9]", "")), // ignore case, spaces, and special characters when sorting
            lastUpdated: metadata.LastUpdated,
            isStale: this.Cache.IsStale(metadata.LastUpdated, this.StaleMinutes)
        );
    }
}
