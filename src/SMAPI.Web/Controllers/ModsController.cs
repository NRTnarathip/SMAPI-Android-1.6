using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StardewModdingAPI.Toolkit.Utilities;
using StardewModdingAPI.Web.Framework.Caching;
using StardewModdingAPI.Web.Framework.Caching.CompatibilityRepo;
using StardewModdingAPI.Web.Framework.ConfigModels;
using StardewModdingAPI.Web.Framework.Storage;
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

    /// <summary>Provides access to raw data storage.</summary>
    private readonly IStorageProvider Storage;

    /// <summary>The number of minutes before which compatibility list data should be considered old.</summary>
    private readonly int StaleMinutes;

    /// <summary>When this server instance last refreshed the mod list cache file.</summary>
    private DateTimeOffset LastCacheUpdate = DateTimeOffset.MinValue;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="cache">The cache in which to store mod metadata.</param>
    /// <param name="storage">Provides access to raw data storage.</param>
    /// <param name="configProvider">The config settings for mod update checks.</param>
    public ModsController(ICompatibilityCacheRepository cache, IStorageProvider storage, IOptions<ModCompatibilityListConfig> configProvider)
    {
        ModCompatibilityListConfig config = configProvider.Value;

        this.Cache = cache;
        this.Storage = storage;
        this.StaleMinutes = config.StaleMinutes;
    }

    /// <summary>Display information for all mods.</summary>
    [HttpGet]
    [Route("mods")]
    public async Task<ViewResult> Index()
    {
        return this.View("Index", await this.BuildModelAsync());
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Build a view model for the current mod compatibility list data.</summary>
    private async Task<ModListModel> BuildModelAsync()
    {
        // get metadata
        if (!this.Cache.TryGetCacheMetadata(out Cached<CompatibilityListMetadata>? metadata))
            return new ModListModel(null, [], lastUpdated: DateTimeOffset.UtcNow, isStale: true);

        // fetch data
        (string? fetchUri, ModModel[]? overrideData) = await this.FetchDataAsync(metadata);

        // build model
        return new ModListModel(
            fetchUri: fetchUri,
            mods: overrideData,
            lastUpdated: metadata.LastUpdated,
            isStale: this.Cache.IsStale(metadata.LastUpdated, this.StaleMinutes)
        );
    }

    /// <summary>Get the cached data to send to the frontend, creating it if needed.</summary>
    /// <param name="metadata">The mod list cache metadata.</param>
    private async Task<(string? FetchUri, ModModel[]? OverrideData)> FetchDataAsync(Cached<CompatibilityListMetadata> metadata)
    {
        const string cacheKey = "mod-list";

        // fetch file if still fresh
        StoredFileInfo? fileInfo = this.LastCacheUpdate >= metadata.LastRequested
            ? await this.Storage.GetAsync(cacheKey, false)
            : null;

        // recreate cache file if needed
        if (fileInfo?.Success != true)
        {
            var mods = this.Cache
                .GetMods()
                .Select(mod => new ModModel(mod.Data))
                .OrderBy(p => PathUtilities.CreateSlug(p.Name)?.ToLower() ?? ""); // ignore case, spaces, and special characters when sorting

            await this.Storage.SaveToJsonAsync(cacheKey, mods);
            this.LastCacheUpdate = DateTimeOffset.UtcNow;

            fileInfo = await this.Storage.GetAsync(cacheKey, false);
        }

        // get info
        string? fetchUri = fileInfo.FetchUri;
        ModModel[]? overrideData = fileInfo.FetchUri is null && fileInfo.FetchedData != null
            ? JsonConvert.DeserializeObject<ModModel[]>(fileInfo.FetchedData)
            : null;
        return (fetchUri, overrideData);
    }
}
