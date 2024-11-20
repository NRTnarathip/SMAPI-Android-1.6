using System.Collections.Generic;
using StardewModdingAPI.Toolkit.Framework.UpdateData;

namespace StardewModdingAPI.Toolkit.Framework.Clients.CompatibilityRepo;

/// <summary>A mod entry in the compatibility list.</summary>
public class ModCompatibilityEntry
{
    /*********
    ** Accessors
    *********/
    /// <summary>The mod's unique ID. If the mod has alternate/old IDs, they're listed in latest to oldest order.</summary>
    public string[] ID { get; }

    /// <summary>The mod's display name. If the mod has multiple names, the first one is the most canonical name.</summary>
    public string[] Name { get; }

    /// <summary>The mod's author name. If the author has multiple names, the first one is the most canonical name.</summary>
    public string[] Author { get; }

    /// <summary>The mod ID on Nexus.</summary>
    public int? NexusID { get; }

    /// <summary>The mod ID in the Chucklefish mod repo.</summary>
    public int? ChucklefishID { get; }

    /// <summary>The mod ID in the CurseForge mod repo.</summary>
    public int? CurseForgeID { get; }

    /// <summary>The mod ID in the ModDrop mod repo.</summary>
    public int? ModDropID { get; }

    /// <summary>The GitHub repository in the form 'owner/repo'.</summary>
    public string? GitHubRepo { get; }

    /// <summary>The URL to a non-GitHub source repo.</summary>
    public string? CustomSourceUrl { get; }

    /// <summary>The custom mod page URL (if applicable).</summary>
    public string? CustomUrl { get; }

    /// <summary>The name of the mod which loads this content pack, if applicable.</summary>
    public string? ContentPackFor { get; }

    /// <summary>The mod's compatibility with the latest stable version of the game.</summary>
    public ModCompatibilityInfo Compatibility { get; }

    /// <summary>The human-readable warnings for players about this mod.</summary>
    public string[] Warnings { get; }

    /// <summary>Special notes intended for developers who maintain unofficial updates or submit pull requests.</summary>
    public string? DevNote { get; }

    /// <summary>The data overrides to apply to the mod's manifest or remote mod page data, if any.</summary>
    public ModDataOverrideEntry? Overrides { get; }

    /// <summary>The link anchor for the mod entry in the compatibility list.</summary>
    public string? Anchor { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="id">The mod's unique ID. If the mod has alternate/old IDs, they're listed in latest to oldest order.</param>
    /// <param name="name">The mod's display name. If the mod has multiple names, the first one is the most canonical name.</param>
    /// <param name="author">The mod's author name. If the author has multiple names, the first one is the most canonical name.</param>
    /// <param name="nexusId">The mod ID on Nexus.</param>
    /// <param name="chucklefishId">The mod ID in the Chucklefish mod repo.</param>
    /// <param name="curseForgeId">The mod ID in the CurseForge mod repo.</param>
    /// <param name="modDropId">The mod ID in the ModDrop mod repo.</param>
    /// <param name="githubRepo">The GitHub repository in the form 'owner/repo'.</param>
    /// <param name="customSourceUrl">The URL to a non-GitHub source repo.</param>
    /// <param name="customUrl">The custom mod page URL (if applicable).</param>
    /// <param name="contentPackFor">The name of the mod which loads this content pack, if applicable.</param>
    /// <param name="compatibility">The mod's compatibility with the latest stable version of the game.</param>
    /// <param name="warnings">The human-readable warnings for players about this mod.</param>
    /// <param name="devNote">Special notes intended for developers who maintain unofficial updates or submit pull requests.</param>
    /// <param name="overrides">The data overrides to apply to the mod's manifest or remote mod page data, if any.</param>
    /// <param name="anchor">The link anchor for the mod entry in the compatibility list.</param>
    public ModCompatibilityEntry(string[] id, string[] name, string[] author, int? nexusId, int? chucklefishId, int? curseForgeId, int? modDropId, string? githubRepo, string? customSourceUrl, string? customUrl, string? contentPackFor, ModCompatibilityInfo compatibility, string[] warnings, string? devNote, ModDataOverrideEntry? overrides, string? anchor)
    {
        this.ID = id;
        this.Name = name;
        this.Author = author;
        this.NexusID = nexusId;
        this.ChucklefishID = chucklefishId;
        this.CurseForgeID = curseForgeId;
        this.ModDropID = modDropId;
        this.GitHubRepo = githubRepo;
        this.CustomSourceUrl = customSourceUrl;
        this.CustomUrl = customUrl;
        this.ContentPackFor = contentPackFor;
        this.Compatibility = compatibility;
        this.Warnings = warnings;
        this.DevNote = devNote;
        this.Overrides = overrides;
        this.Anchor = anchor;
    }

    /// <summary>Get the web URLs for the mod pages, if any.</summary>
    public IEnumerable<KeyValuePair<ModSiteKey, string>> GetModPageUrls()
    {
        bool anyFound = false;

        // normal mod pages
        if (this.NexusID.HasValue)
        {
            anyFound = true;
            yield return new KeyValuePair<ModSiteKey, string>(ModSiteKey.Nexus, $"https://www.nexusmods.com/stardewvalley/mods/{this.NexusID}");
        }
        if (this.ModDropID.HasValue)
        {
            anyFound = true;
            yield return new KeyValuePair<ModSiteKey, string>(ModSiteKey.ModDrop, $"https://www.moddrop.com/stardew-valley/mod/{this.ModDropID}");
        }
        if (this.CurseForgeID.HasValue)
        {
            anyFound = true;
            yield return new KeyValuePair<ModSiteKey, string>(ModSiteKey.CurseForge, $"https://www.curseforge.com/projects/{this.CurseForgeID}");
        }
        if (this.ChucklefishID.HasValue)
        {
            anyFound = true;
            yield return new KeyValuePair<ModSiteKey, string>(ModSiteKey.Chucklefish, $"https://community.playstarbound.com/resources/{this.ChucklefishID}");
        }

        // custom URL
        if (!anyFound && !string.IsNullOrWhiteSpace(this.CustomUrl))
        {
            anyFound = true;
            yield return new KeyValuePair<ModSiteKey, string>(ModSiteKey.Unknown, this.CustomUrl);
        }

        // fallback
        if (!anyFound && !string.IsNullOrWhiteSpace(this.GitHubRepo))
            yield return new KeyValuePair<ModSiteKey, string>(ModSiteKey.GitHub, $"https://github.com/{this.GitHubRepo}/releases");
    }
}
