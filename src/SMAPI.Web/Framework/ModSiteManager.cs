using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StardewModdingAPI.Toolkit;
using StardewModdingAPI.Toolkit.Framework.Clients.CompatibilityRepo;
using StardewModdingAPI.Toolkit.Framework.UpdateData;
using StardewModdingAPI.Web.Framework.Clients;

namespace StardewModdingAPI.Web.Framework;

/// <summary>Handles fetching data from mod sites.</summary>
internal class ModSiteManager
{
    /*********
    ** Fields
    *********/
    /// <summary>The mod sites which provide mod metadata.</summary>
    private readonly IDictionary<ModSiteKey, IModSiteClient> ModSites;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modSites">The mod sites which provide mod metadata.</param>
    public ModSiteManager(IModSiteClient[] modSites)
    {
        this.ModSites = modSites.ToDictionary(p => p.SiteKey);
    }

    /// <summary>Get the mod info for an update key.</summary>
    /// <param name="updateKey">The namespaced update key.</param>
    public async Task<IModPage> GetModPageAsync(UpdateKey updateKey)
    {
        if (!updateKey.LooksValid)
            return new GenericModPage(updateKey.Site, updateKey.ID!).SetError(RemoteModStatus.DoesNotExist, $"Invalid update key '{updateKey}'.");

        // get site
        if (!this.ModSites.TryGetValue(updateKey.Site, out IModSiteClient? client))
            return new GenericModPage(updateKey.Site, updateKey.ID).SetError(RemoteModStatus.DoesNotExist, $"There's no mod site with key '{updateKey.Site}'. Expected one of [{string.Join(", ", this.ModSites.Keys)}].");

        // fetch mod
        IModPage? mod;
        try
        {
            mod = await client.GetModData(updateKey.ID);
        }
        catch (Exception ex)
        {
            mod = new GenericModPage(updateKey.Site, updateKey.ID).SetError(RemoteModStatus.TemporaryError, ex.ToString());
        }

        // handle errors
        return mod ?? new GenericModPage(updateKey.Site, updateKey.ID).SetError(RemoteModStatus.DoesNotExist, $"Found no {updateKey.Site} mod with ID '{updateKey.ID}'.");
    }

    /// <summary>Parse version info for the given mod page info.</summary>
    /// <param name="page">The mod page info.</param>
    /// <param name="updateKey">The update key to match in available files.</param>
    /// <param name="mapRemoteVersions">The changes to apply to remote versions for update checks.</param>
    /// <param name="allowNonStandardVersions">Whether to allow non-standard versions.</param>
    public ModInfoModel GetPageVersions(IModPage page, UpdateKey updateKey, bool allowNonStandardVersions, ChangeDescriptor? mapRemoteVersions)
    {
        // get ID to show in errors
        string displayId = page.RequireSubkey
            ? page.Id + updateKey.Subkey
            : page.Id;

        // validate
        ModInfoModel model = new();
        if (!page.IsValid)
            return model.SetError(page.Status, page.Error);
        if (page.RequireSubkey && updateKey.Subkey is null)
            return model.SetError(RemoteModStatus.RequiredSubkeyMissing, $"The {page.Site} mod with ID '{displayId}' requires an update subkey indicating which mod to fetch.");

        // add basic info (unless it's a manifest, in which case the 'mod page' is the JSON file)
        if (updateKey.Site != ModSiteKey.UpdateManifest)
            model.SetBasicInfo(page.Name, page.Url);

        // fetch versions
        bool hasVersions = this.TryGetLatestVersions(page, updateKey.Subkey, allowNonStandardVersions, mapRemoteVersions, out ISemanticVersion? mainVersion, out ISemanticVersion? previewVersion, out string? mainModPageUrl, out string? previewModPageUrl);
        if (!hasVersions)
            return model.SetError(RemoteModStatus.InvalidData, $"The {page.Site} mod with ID '{displayId}' has no valid versions.");

        // apply mod page info
        model.SetBasicInfo(
            name: page.GetName(updateKey.Subkey) ?? page.Name,
            url: page.GetUrl(updateKey.Subkey) ?? page.Url
        );

        // return info
        return model
            .SetMainVersion(mainVersion!, mainModPageUrl)
            .SetPreviewVersion(previewVersion, previewModPageUrl);
    }

    /// <summary>Get a semantic local version for update checks.</summary>
    /// <param name="version">The version to parse.</param>
    /// <param name="map">Changes to apply to the raw version, if any.</param>
    /// <param name="allowNonStandard">Whether to allow non-standard versions.</param>
    public ISemanticVersion? GetMappedVersion(string? version, ChangeDescriptor? map, bool allowNonStandard)
    {
        // try mapped version
        string? rawNewVersion = this.GetRawMappedVersion(version, map);
        if (SemanticVersion.TryParse(rawNewVersion, allowNonStandard, out ISemanticVersion? parsedNew))
            return parsedNew;

        // return original version
        return SemanticVersion.TryParse(version, allowNonStandard, out ISemanticVersion? parsedOld)
            ? parsedOld
            : null;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the mod version numbers for the given mod.</summary>
    /// <param name="mod">The mod to check.</param>
    /// <param name="subkey">The optional update subkey to match in available files. (If no file names or descriptions contain the subkey, it'll be ignored.)</param>
    /// <param name="allowNonStandardVersions">Whether to allow non-standard versions.</param>
    /// <param name="mapRemoteVersions">The changes to apply to remote versions for update checks.</param>
    /// <param name="main">The main mod version.</param>
    /// <param name="preview">The latest prerelease version, if newer than <paramref name="main"/>.</param>
    /// <param name="mainModPageUrl">The mod page URL from which <paramref name="main"/> can be downloaded, if different from the <see cref="mod"/>'s URL.</param>
    /// <param name="previewModPageUrl">The mod page URL from which <paramref name="preview"/> can be downloaded, if different from the <see cref="mod"/>'s URL.</param>
    private bool TryGetLatestVersions(IModPage? mod, string? subkey, bool allowNonStandardVersions, ChangeDescriptor? mapRemoteVersions, [NotNullWhen(true)] out ISemanticVersion? main, out ISemanticVersion? preview, out string? mainModPageUrl, out string? previewModPageUrl)
    {
        main = null;
        preview = null;
        mainModPageUrl = null;
        previewModPageUrl = null;
        if (mod is null)
            return false;

        // parse all versions from the mod page
        IEnumerable<(IModDownload? download, ISemanticVersion? version)> GetAllVersions()
        {
            ISemanticVersion? ParseAndMapVersion(string? raw)
            {
                raw = this.NormalizeVersion(raw);
                return this.GetMappedVersion(raw, mapRemoteVersions, allowNonStandardVersions);
            }

            // get mod version
            ISemanticVersion? modVersion = ParseAndMapVersion(mod.Version);
            if (modVersion != null)
                yield return (download: null, version: modVersion);

            // get file versions
            foreach (IModDownload download in mod.Downloads)
            {
                ISemanticVersion? cur = ParseAndMapVersion(download.Version);
                if (cur != null)
                    yield return (download, cur);
            }
        }
        var versions = GetAllVersions()
            .OrderByDescending(p => p.version, SemanticVersionComparer.Instance)
            .ToArray();

        // get main + preview versions
        void TryGetVersions([NotNullWhen(true)] out ISemanticVersion? mainVersion, out ISemanticVersion? previewVersion, out string? mainUrl, out string? previewUrl, Func<(IModDownload? download, ISemanticVersion? version), bool>? filter = null)
        {
            mainVersion = null;
            previewVersion = null;
            mainUrl = null;
            previewUrl = null;

            // get latest main + preview version
            foreach ((IModDownload? download, ISemanticVersion? version) entry in versions)
            {
                if (entry.version is null || filter?.Invoke(entry) == false)
                    continue;

                if (entry.version.IsPrerelease())
                {
                    if (previewVersion is null)
                    {
                        previewVersion = entry.version;
                        previewUrl = entry.download?.ModPageUrl;
                    }
                }
                else
                {
                    mainVersion = entry.version;
                    mainUrl = entry.download?.ModPageUrl;
                    break; // any others will be older since entries are sorted by version
                }
            }

            // normalize values
            if (previewVersion is not null)
            {
                if (mainVersion is null)
                {
                    // if every version is prerelease, latest one is the main version
                    mainVersion = previewVersion;
                    mainUrl = previewUrl;
                }
                if (!previewVersion.IsNewerThan(mainVersion))
                {
                    previewVersion = null;
                    previewUrl = null;
                }
            }
        }

        // get versions for subkey
        if (subkey is not null)
            TryGetVersions(out main, out preview, out mainModPageUrl, out previewModPageUrl, filter: entry => entry.download?.MatchesSubkey(subkey) == true);

        // fallback to non-subkey versions
        if (main is null && !mod.RequireSubkey)
            TryGetVersions(out main, out preview, out mainModPageUrl, out previewModPageUrl);
        return main != null;
    }

    /// <summary>Get a semantic local version for update checks.</summary>
    /// <param name="version">The version to map.</param>
    /// <param name="map">Changes to apply to the raw version, if any.</param>
    private string? GetRawMappedVersion(string? version, ChangeDescriptor? map)
    {
        if (version == null || map?.HasChanges != true)
            return version;

        var mapped = new List<string> { version };
        map.Apply(mapped);

        return mapped.FirstOrDefault();
    }

    /// <summary>Normalize a version string.</summary>
    /// <param name="version">The version to normalize.</param>
    private string? NormalizeVersion(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return null;

        version = version.Trim();
        if (Regex.IsMatch(version, @"^v\d", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)) // common version prefix
            version = version.Substring(1);

        return version;
    }
}
