using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StardewModdingAPI.Toolkit.Framework.Clients.CompatibilityRepo;
using StardewModdingAPI.Toolkit.Framework.GameScanning;
using StardewModdingAPI.Toolkit.Framework.ModData;
using StardewModdingAPI.Toolkit.Framework.ModScanning;
using StardewModdingAPI.Toolkit.Framework.UpdateData;
using StardewModdingAPI.Toolkit.Serialization;

namespace StardewModdingAPI.Toolkit;

/// <summary>A convenience wrapper for the various tools.</summary>
public class ModToolkit
{
    /*********
    ** Fields
    *********/
    /// <summary>The default HTTP user agent for the toolkit.</summary>
    private readonly string UserAgent;

    /// <summary>Maps vendor keys (like <c>Nexus</c>) to their mod URL template (where <c>{0}</c> is the mod ID). This doesn't affect update checks, which defer to the remote web API.</summary>
    private readonly Dictionary<ModSiteKey, string> VendorModUrls = new()
    {
        [ModSiteKey.Chucklefish] = "https://community.playstarbound.com/resources/{0}",
        [ModSiteKey.CurseForge] = "https://www.curseforge.com/projects/{0}",
        [ModSiteKey.GitHub] = "https://github.com/{0}/releases",
        [ModSiteKey.ModDrop] = "https://www.moddrop.com/stardew-valley/mods/{0}",
        [ModSiteKey.Nexus] = "https://www.nexusmods.com/stardewvalley/mods/{0}"
    };


    /*********
    ** Accessors
    *********/
    /// <summary>Encapsulates SMAPI's JSON parsing.</summary>
    public JsonHelper JsonHelper { get; } = new();


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    public ModToolkit()
    {
        ISemanticVersion version = new SemanticVersion(this.GetType().Assembly.GetName().Version!);
        this.UserAgent = $"SMAPI Mod Handler Toolkit/{version}";
    }

    /// <summary>Find valid Stardew Valley install folders.</summary>
    /// <remarks>This checks default game locations, and on Windows checks the Windows registry for GOG/Steam install data. A folder is considered 'valid' if it contains the Stardew Valley executable for the current OS.</remarks>
    public IEnumerable<DirectoryInfo> GetGameFolders()
    {
        return new GameScanner().Scan();
    }

    /// <summary>Find all default Stardew Valley install folders which exist, regardless of whether they're valid.</summary>
    /// <remarks>This checks default game locations, and on Windows checks the Windows registry for GOG/Steam install data.</remarks>
    public IEnumerable<(DirectoryInfo, GameFolderType)> GetGameFoldersIncludingInvalid()
    {
        return new GameScanner().ScanIncludingInvalid();
    }

    /// <summary>Extract mod metadata from the compatibility list repo.</summary>
    public async Task<ModCompatibilityEntry[]> GetCompatibilityListAsync()
    {
        using CompatibilityRepoClient client = new(this.UserAgent);
        return await client.FetchModsAsync();
    }

    /// <summary>Extract mod metadata from the compatibility list repo by reading a local copy of the repo.</summary>
    /// <param name="gitRepoPath">The full path to the compatibility list repo folder.</param>
    public async Task<ModCompatibilityEntry[]> GetCompatibilityListFromLocalGitFolderAsync(string gitRepoPath)
    {
        using CompatibilityRepoClient client = new(this.UserAgent);
        return await client.FetchModsFromLocalGitFolderAsync(gitRepoPath);
    }

    /// <summary>Get SMAPI's internal mod database.</summary>
    /// <param name="metadataPath">The file path for the SMAPI metadata file.</param>
    public ModDatabase GetModDatabase(string metadataPath)
    {
        MetadataModel metadata = JsonConvert.DeserializeObject<MetadataModel>(File.ReadAllText(metadataPath)) ?? new MetadataModel();
        ModDataRecord[] records = metadata.ModData.Select(pair => new ModDataRecord(pair.Key, pair.Value)).ToArray();
        return new ModDatabase(records, this.GetUpdateUrl);
    }

    /// <summary>Extract information about all mods in the given folder.</summary>
    /// <param name="rootPath">The root folder containing mods.</param>
    /// <param name="useCaseInsensitiveFilePaths">Whether to match file paths case-insensitively, even on Linux.</param>
    public IEnumerable<ModFolder> GetModFolders(string rootPath, bool useCaseInsensitiveFilePaths)
    {
        return new ModScanner(this.JsonHelper).GetModFolders(rootPath, useCaseInsensitiveFilePaths);
    }

    /// <summary>Extract information about all mods in the given folder.</summary>
    /// <param name="rootPath">The root folder containing mods. Only the <paramref name="modPath"/> will be searched, but this field allows it to be treated as a potential mod folder of its own.</param>
    /// <param name="modPath">The mod path to search.</param>
    /// <param name="useCaseInsensitiveFilePaths">Whether to match file paths case-insensitively, even on Linux.</param>
    public IEnumerable<ModFolder> GetModFolders(string rootPath, string modPath, bool useCaseInsensitiveFilePaths)
    {
        return new ModScanner(this.JsonHelper).GetModFolders(rootPath, modPath, useCaseInsensitiveFilePaths);
    }

    /// <summary>Get an update URL for an update key (if valid).</summary>
    /// <param name="updateKey">The update key.</param>
    /// <returns>Returns the URL if the mod site is supported, else <c>null</c>.</returns>
    public string? GetUpdateUrl(string updateKey)
    {
        UpdateKey parsed = UpdateKey.Parse(updateKey);
        if (!parsed.LooksValid)
            return null;

        return this.GetUpdateUrl(parsed.Site, parsed.ID);
    }

    /// <summary>Get an update URL for an update key (if valid).</summary>
    /// <param name="site">The mod site containing the mod.</param>
    /// <param name="id">The mod ID within the site.</param>
    /// <returns>Returns the URL if the mod site is supported, else <c>null</c>.</returns>
    public string? GetUpdateUrl(ModSiteKey site, string id)
    {
        if (this.VendorModUrls.TryGetValue(site, out string? urlTemplate))
            return string.Format(urlTemplate, id);

        return null;
    }
}
