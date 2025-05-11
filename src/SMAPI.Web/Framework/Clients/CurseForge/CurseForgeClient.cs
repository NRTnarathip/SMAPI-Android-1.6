using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pathoschild.Http.Client;
using StardewModdingAPI.Toolkit.Framework.Clients.CurseForgeExport.ResponseModels;
using StardewModdingAPI.Toolkit.Framework.UpdateData;
using StardewModdingAPI.Web.Framework.Caching.CurseForgeExport;
using StardewModdingAPI.Web.Framework.Clients.CurseForge.ResponseModels;

namespace StardewModdingAPI.Web.Framework.Clients.CurseForge;

/// <summary>An HTTP client for fetching mod metadata from the CurseForge API.</summary>
internal class CurseForgeClient : ICurseForgeClient
{
    /*********
    ** Fields
    *********/
    /// <summary>The URL for a CurseForge mod page for the user, where {0} is the mod ID.</summary>
    private readonly string WebModUrl;

    /// <summary>The underlying HTTP client.</summary>
    private readonly IClient Client;

    /// <summary>The cached mod data from the CurseForge export API to use if available.</summary>
    private readonly ICurseForgeExportCacheRepository ExportCache;

    /// <summary>A regex pattern which matches a version number in a CurseForge mod file name.</summary>
    private readonly Regex VersionInNamePattern = new(@"^(?:.+? | *)v?(\d+\.\d+(?:\.\d+)?(?:-.+?)?) *(?:\.(?:zip|rar|7z))?$", RegexOptions.Compiled);


    /*********
    ** Accessors
    *********/
    /// <summary>The unique key for the mod site.</summary>
    public ModSiteKey SiteKey => ModSiteKey.CurseForge;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="userAgent">The user agent for the API client.</param>
    /// <param name="apiUrl">The base URL for the CurseForge API.</param>
    /// <param name="apiKey">The API authentication key.</param>
    /// <param name="webModUrl">The URL for a CurseForge mod page for the user, where {0} is the mod ID.</param>
    /// <param name="exportCache">The cached mod data from the CurseForge export API to use if available.</param>
    public CurseForgeClient(string userAgent, string apiUrl, string apiKey, string webModUrl, ICurseForgeExportCacheRepository exportCache)
    {
        this.Client = new FluentClient(apiUrl)
            .SetUserAgent(userAgent)
            .AddDefault(request => request.WithHeader("x-api-key", apiKey));
        this.WebModUrl = webModUrl;
        this.ExportCache = exportCache;
    }

    /// <summary>Get update check info about a mod.</summary>
    /// <param name="id">The mod ID.</param>
    public async Task<IModPage?> GetModData(string id)
    {
        // get ID
        if (!uint.TryParse(id, out uint parsedId))
            return this.InitModPage(id).SetError(RemoteModStatus.DoesNotExist, $"The value '{id}' isn't a valid CurseForge mod ID, must be an integer ID.");

        // To minimize time users spend waiting for the update check result, we fetch the mod
        // from these sources in order of priority:
        //
        // 1. CurseForge export API:
        //    This is a special endpoint provided by CurseForge specifically for SMAPI's update
        //    checks. It returns a cached view of every Stardew Valley mod, so we don't need to
        //    submit separate requests for each mod.
        //
        //   2. CurseForge API:
        //      Though mostly superseded by the export API, this is the fallback if the export
        //      isn't available for some reason.
        return
            (
                this.ExportCache.IsLoaded && parsedId != 898372/* SMAPI isn't in the export since it's not technically a mod */
                    ? this.GetModFromExportData(parsedId)
                    : await this.GetModFromApiAsync(parsedId)
            )
            ?? this.InitModPage(id).SetError(RemoteModStatus.DoesNotExist, "Found no CurseForge mod with this ID.");
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        this.Client.Dispose();
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Initialize an empty mod page model.</summary>
    /// <param name="id">The CurseForge mod ID.</param>
    private IModPage InitModPage(uint id)
    {
        return new GenericModPage(this.SiteKey, id.ToString());
    }

    /// <summary>Initialize an empty mod page model.</summary>
    /// <param name="id">The CurseForge mod ID.</param>
    private IModPage InitModPage(string id)
    {
        return new GenericModPage(this.SiteKey, id);
    }

    /// <summary>Get metadata about a mod by searching the CurseForge export API data.</summary>
    /// <param name="id">The CurseForge mod ID.</param>
    /// <returns>Returns the mod info if found, else <c>null</c>.</returns>
    private IModPage? GetModFromExportData(uint id)
    {
        // skip if no data available
        if (!this.ExportCache.IsLoaded || !this.ExportCache.TryGetMod(id, out CurseForgeModExport? data))
            return null;

        // get downloads
        var downloads = new List<IModDownload>();
        foreach (CurseForgeFileExport file in data.Files)
        {
            // CurseForge imports files from Nexus into groups, but files uploaded manually still have a group type
            // set to null or zero.
            switch (file.FileGroupType)
            {
                case null:
                case CurseForgeFileGroupType.None:
                case CurseForgeFileGroupType.Main:
                case CurseForgeFileGroupType.Optional:
                    downloads.Add(
                        new GenericModDownload(file.DisplayName ?? file.FileName ?? file.Id.ToString(), null, this.GetRawVersion(null, file.FileName))
                    );
                    break;
            }
        }

        // yield info
        return this.InitModPage(id)
            .SetInfo(
                name: data.Name ?? id.ToString(),
                version: null,
                url: data.ModPageUrl ?? this.GetModUrl(id),
                downloads: downloads.ToArray()
            );
    }

    /// <summary>Get metadata about a mod from the CurseForge API.</summary>
    /// <param name="id">The CurseForge mod ID.</param>
    /// <returns>Returns the mod info if found, else <c>null</c>.</returns>
    private async Task<IModPage?> GetModFromApiAsync(uint id)
    {
        // get raw data
        ModModel? mod;
        try
        {
            ResponseModel<ModModel> response = await this.Client
                .GetAsync($"mods/{id}")
                .As<ResponseModel<ModModel>>();
            mod = response.Data;
        }
        catch (ApiException ex) when (ex.Status == HttpStatusCode.NotFound)
        {
            return null;
        }

        // get downloads
        List<IModDownload> downloads = [];
        foreach (ModFileModel file in mod.LatestFiles)
        {
            downloads.Add(
                new GenericModDownload(name: file.DisplayName ?? file.FileName, description: null, version: this.GetRawVersion(file.DisplayName, file.FileName))
            );
        }

        // return info
        return this.InitModPage(id).SetInfo(name: mod.Name, version: null, url: mod.Links.WebsiteUrl, downloads: downloads);
    }

    /// <summary>Get a raw version string for a mod file, if available.</summary>
    /// <param name="displayName">The file's display name.</param>
    /// <param name="fileName">The file's internal name.</param>
    private string? GetRawVersion(string? displayName, string? fileName)
    {
        // get raw version
        Match match = this.VersionInNamePattern.Match(displayName ?? "");
        if (!match.Success)
            match = this.VersionInNamePattern.Match(fileName ?? "");
        if (!match.Success)
            return null;

        // fix auto-synced prerelease versions having a double version in the name (like "2.4.0-alpha.20240818-2-4-0-alpha-20240818")
        string version = match.Groups[1].Value;
        if (version.Length > 2 && version.Length % 2 == 1)
        {
            int splitIndex = version.Length / 2;
            if (version[splitIndex] == '-')
            {
                string left = version[..splitIndex];
                string right = version[(splitIndex + 1)..];

                if (left.Replace('.', '-') == right)
                    version = left;
            }
        }

        return version;
    }

    /// <summary>Get the full mod page URL for a given ID.</summary>
    /// <param name="id">The mod ID.</param>
    private string GetModUrl(uint id)
    {
        return string.Format(this.WebModUrl, id);
    }
}
