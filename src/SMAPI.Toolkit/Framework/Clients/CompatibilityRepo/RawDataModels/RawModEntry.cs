using System;

namespace StardewModdingAPI.Toolkit.Framework.Clients.CompatibilityRepo.RawDataModels;

/// <summary>The compatibility metadata for a mod in the raw data.</summary>
public class RawModEntry
{
    /*********
    ** Properties
    *********/
    /****
    ** Main fields
    ****/
    /// <summary>The normalised display name for the mod. If the mod has alternate names, list them comma-separated after the main name.</summary>
    public string? Name { get; set; }

    /// <summary>The normalized display name for the author. If the author has alternate names, list them comma-separated after the main name.</summary>
    public string? Author { get; set; }

    /// <summary>The unique mod ID, as listed in its manifest.json file. If the mod has alternate or former IDs, list them comma-separated after the main one (ideally in latest to oldest order). For very old mods with no ID, use 'none' to disable validation checks.</summary>
    public string? Id { get; set; }

    /// <summary>The mod's unique ID on Nexus, or null if it has none. This is the number in the mod page's URL.</summary>
    public int? Nexus { get; set; }

    /// <summary>The mod's GitHub repository in the form owner/repo, or null if it has none.</summary>
    public string? GitHub { get; set; }

    /****
    ** Secondary fields
    ****/
    /// <summary>The mod's unique ID in the legacy Chucklefish mod repository (if any). This is the value shown in the mod page's URL.</summary>
    public int? Chucklefish { get; set; }

    /// <summary>The mod's unique ID on CurseForge (if any). This is the value shown on the mod page next to "Project ID".</summary>
    public int? Curse { get; set; }

    /// <summary>The mod's unique ID on ModDrop (if any). This is the value shown in the mod page's URL.</summary>
    public int? ModDrop { get; set; }

    /// <summary>The arbitrary mod URL, if the mod isn't on a mod site supported by more specific fields like `nexus`. This should be avoided if possible, since this makes cross-referencing much more difficult.</summary>
    public string? Url { get; set; }

    /// <summary>An arbitrary source code URL, if not on GitHub. Avoid if possible, since this makes cross-referencing more difficult.</summary>
    public string? Source { get; set; }

    /// <summary>Custom text indicating compatibility issues with the mod (e.g. not compatible with Linux/Mac).</summary>
    public string[]? Warnings { get; set; }

    /// <summary>Special notes intended for developers who maintain unofficial updates or submit pull requests.</summary>
    public string? DeveloperNotes { get; set; }

    /****
    ** Compatibility
    ****/
    /// <summary>The mod's compatibility with the latest versions of Stardew Valley and SMAPI.</summary>
    public string? Status { get; set; }

    /// <summary>A custom description of the mod's compatibility, in Markdown format. This should be left blank if it's covered by more specific fields like `brokeIn` or `unofficialUpdate`.</summary>
    public string? Summary { get; set; }

    /// <summary>The SMAPI, Stardew Valley, or other release which broke this mod if applicable. This should include both the name and version, like 'Stardew Valley 1.6.9'.</summary>
    public string? BrokeIn { get; set; }

    /// <summary>The unofficial update which fixes compatibility with the latest Stardew Valley and SMAPI versions.</summary>
    public RawModUnofficialUpdate? UnofficialUpdate { get; set; }

    /****
    ** Content packs only
    ****/
    /// <summary>The unique ID of the framework mod for which this is a content pack.</summary>
    public string? ContentPackFor { get; set; }

    /****
    ** Data overrides
    ****/
    /// <summary>The data overrides to apply to the mod's manifest or remote mod page data, if any.</summary>
    public RawModDataOverride[]? OverrideModData { get; set; }


    /*********
    ** Public methods
    *********/
    /// <summary>Get whether this mod has a source code link.</summary>
    public bool HasSource()
    {
        return
            !string.IsNullOrWhiteSpace(this.GitHub)
            || !string.IsNullOrWhiteSpace(this.Source);
    }

    /// <summary>Get the mod's compatibility status based on its fields.</summary>
    public ModCompatibilityStatus GetStatus()
    {
        if (!Enum.TryParse(this.Status, true, out ModCompatibilityStatus status))
        {
            if (this.UnofficialUpdate != null)
                status = ModCompatibilityStatus.Unofficial;
            else if (this.BrokeIn != null)
                status = ModCompatibilityStatus.Broken;
            else
                status = ModCompatibilityStatus.Ok;
        }

        return status;
    }

    /// <summary>Get the compatibility summary as shown on the compatibility list.</summary>
    /// <param name="summary">The compatibility summary.</param>
    /// <param name="mayBeMarkdown">Whether the summary may contain Markdown formatting.</param>
    public void GetCompatibilitySummary(out string summary, out bool mayBeMarkdown)
    {
        // get mod info
        bool hasSource = this.HasSource();
        ModCompatibilityStatus status = this.GetStatus();

        // get icon
        char statusIcon = status switch
        {
            ModCompatibilityStatus.Unofficial or ModCompatibilityStatus.Workaround => '⚠',
            ModCompatibilityStatus.Broken when hasSource => '↻',
            ModCompatibilityStatus.Broken or ModCompatibilityStatus.Obsolete or ModCompatibilityStatus.Abandoned => '✖',
            _ => '✓'
        };

        // get summary
        if (this.Summary is not null)
        {
            summary = $"{statusIcon} {this.Summary}";
            mayBeMarkdown = true;
        }
        else
        {
            mayBeMarkdown = false;

            switch (status)
            {
                case ModCompatibilityStatus.Ok:
                    summary = "use latest version.";
                    break;

                case ModCompatibilityStatus.Optional:
                    summary = "use optional download.";
                    break;

                case ModCompatibilityStatus.Unofficial:
                    summary = $"broken, use [unofficial version]({this.UnofficialUpdate?.Url})";
                    if (this.UnofficialUpdate?.Version != null)
                        summary += $" (<small>{this.UnofficialUpdate.Version}</small>)";
                    summary += '.';
                    mayBeMarkdown = true;
                    break;

                case ModCompatibilityStatus.Workaround:
                    summary = "broken [**error:** should specify summary].";
                    mayBeMarkdown = true;
                    break;

                case ModCompatibilityStatus.Broken:
                    summary = hasSource
                        ? "broken, not updated yet."
                        : "broken, not open-source.";
                    break;

                case ModCompatibilityStatus.Obsolete:
                    summary = "remove this mod (obsolete).";
                    break;

                case ModCompatibilityStatus.Abandoned:
                    summary = "remove this mod (no longer maintained).";
                    break;

                default:
                    summary = $"[**error:** unknown status '{status}'.]";
                    break;
            }
        }
    }
}
