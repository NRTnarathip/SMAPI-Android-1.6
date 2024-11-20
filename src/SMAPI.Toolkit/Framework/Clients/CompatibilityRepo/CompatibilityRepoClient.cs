using System;
using System.Linq;
using System.Threading.Tasks;
using Markdig;
using Pathoschild.Http.Client;
using StardewModdingAPI.Toolkit.Framework.Clients.CompatibilityRepo.Internal;
using StardewModdingAPI.Toolkit.Framework.Clients.CompatibilityRepo.RawDataModels;
using StardewModdingAPI.Toolkit.Framework.MarkdownExtensions;
using StardewModdingAPI.Toolkit.Utilities;

namespace StardewModdingAPI.Toolkit.Framework.Clients.CompatibilityRepo;

/// <summary>An HTTP client for fetching data from the mod compatibility repo.</summary>
public class CompatibilityRepoClient : IDisposable
{
    /*********
    ** Fields
    *********/
    /// <summary>The underlying HTTP client.</summary>
    private readonly IClient Client;

    /// <summary>The Markdown pipeline with which to format Markdown summaries.</summary>
    private readonly MarkdownPipeline MarkdownPipeline;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="fetchUrl">The full URL of the JSON file to fetch.</param>
    /// <param name="userAgent">The user agent for the API client.</param>
    public CompatibilityRepoClient(string userAgent, string fetchUrl = "https://raw.githubusercontent.com/Pathoschild/SmapiCompatibilityList/refs/heads/develop/data/data.jsonc")
    {
        this.Client = new FluentClient(fetchUrl).SetUserAgent(userAgent);
        this.MarkdownPipeline = new MarkdownPipelineBuilder()
            .Use(new ExpandCompatibilityListAnchorLinksExtension())
            .Build();
    }

    /// <summary>Fetch mods from the compatibility list.</summary>
    public async Task<ModCompatibilityEntry[]> FetchModsAsync()
    {
        RawCompatibilityList response = await this.Client
            .GetAsync(null)
            .WithFilter(new ForceJsonResponseTypeFilter())
            .As<RawCompatibilityList>();

        return
            (response.Mods ?? Array.Empty<RawModEntry>())
            .Concat(response.BrokenContentPacks ?? Array.Empty<RawModEntry>())
            .Select(this.ParseRawModEntry)
            .ToArray();
    }

    /// <summary>Get the inline HTML produced by a Markdown string in a compatibility repo field.</summary>
    /// <param name="markdown">The Markdown to parse.</param>
    /// <remarks>This is a low-level method. Most code should use <see cref="FetchModsAsync"/> instead.</remarks>
    public string ParseMarkdownToInlineHtml(string markdown)
    {
        string html = Markdown.ToHtml(markdown, this.MarkdownPipeline);

        // Markdown wraps all content with <p></p>, and there's no non-hacky way to disable it.
        // We need to strip them since the content is shown inline.
        html = html.Trim();
        if (html.StartsWith("<p>", StringComparison.OrdinalIgnoreCase) && html.EndsWith("</p>", StringComparison.OrdinalIgnoreCase) && html.IndexOf("<p>", 3, StringComparison.OrdinalIgnoreCase) == -1)
            html = html.Substring(3, html.Length - 7);

        return html;
    }

    /// <summary>Parse a mod compatibility entry.</summary>
    /// <param name="rawModEntry">The HTML compatibility entries.</param>
    /// <remarks>This is a low-level method. Most code should use <see cref="FetchModsAsync"/> instead.</remarks>
    public ModCompatibilityEntry ParseRawModEntry(RawModEntry rawModEntry)
    {
        // parse main fields
        string[] modIds = this.GetCsv(rawModEntry.Id);
        string[] modNames = this.GetCsv(rawModEntry.Name);
        string[] authorNames = this.GetCsv(rawModEntry.Author);
        ModCompatibilityStatus status = rawModEntry.GetStatus();
        rawModEntry.GetCompatibilitySummary(out string summary, out bool hasMarkdown);

        // get HTML summary
        string? htmlSummary = null;
        if (hasMarkdown)
        {
            htmlSummary = this.ParseMarkdownToInlineHtml(summary);
            if (htmlSummary == summary)
                htmlSummary = null;
        }

        // build model
        return new ModCompatibilityEntry(
            id: modIds,
            name: modNames,
            author: authorNames,
            nexusId: rawModEntry.Nexus,
            chucklefishId: rawModEntry.Chucklefish,
            curseForgeId: rawModEntry.Curse,
            modDropId: rawModEntry.ModDrop,
            githubRepo: rawModEntry.GitHub,
            customSourceUrl: rawModEntry.Source,
            customUrl: rawModEntry.Url,
            contentPackFor: rawModEntry.ContentPackFor,
            compatibility: new ModCompatibilityInfo(
                status: status,
                summary: summary,
                htmlSummary: htmlSummary,
                brokeIn: rawModEntry.BrokeIn,
                unofficialVersion: this.GetSemanticVersion(rawModEntry.UnofficialUpdate?.Version),
                unofficialUrl: rawModEntry.UnofficialUpdate?.Url
            ),
            warnings: rawModEntry.Warnings ?? Array.Empty<string>(),
            devNote: rawModEntry.DeveloperNotes,
            overrides: this.ParseOverrideEntries(modIds, rawModEntry.OverrideModData),
            anchor: PathUtilities.CreateSlug(modNames.FirstOrDefault())?.ToLower()
        );
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        this.Client.Dispose();
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Parse valid mod data override entries.</summary>
    /// <param name="modIds">The mod's unique IDs.</param>
    /// <param name="overrides">The raw data override entries to parse.</param>
    private ModDataOverrideEntry? ParseOverrideEntries(string[] modIds, RawModDataOverride[]? overrides)
    {
        if (overrides?.Length is not > 0)
            return null;

        ModDataOverrideEntry parsed = new() { Ids = modIds };
        foreach (RawModDataOverride @override in overrides)
        {
            switch (@override.Type?.ToLower())
            {
                case "updatekey":
                    parsed.ChangeUpdateKeys ??= new(raw => raw);
                    parsed.ChangeUpdateKeys.AddChange(@override.From, @override.To);
                    break;

                case "localversion":
                    parsed.ChangeLocalVersions ??= new(raw => SemanticVersion.TryParse(raw, out ISemanticVersion? version) ? version.ToString() : raw);
                    parsed.ChangeLocalVersions.AddChange(@override.From, @override.To);
                    break;

                case "remoteversion":
                    parsed.ChangeRemoteVersions ??= new(raw => SemanticVersion.TryParse(raw, out ISemanticVersion? version) ? version.ToString() : raw);
                    parsed.ChangeRemoteVersions.AddChange(@override.From, @override.To);
                    break;
            }
        }

        return parsed;
    }

    /// <summary>Parse a raw value as a comma-delimited list of strings.</summary>
    /// <param name="rawValue">The raw value to parse.</param>
    private string[] GetCsv(string? rawValue)
    {
        return !string.IsNullOrWhiteSpace(rawValue)
            ? rawValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray()
            : Array.Empty<string>();
    }

    /// <summary>Get a raw value as a semantic version.</summary>
    /// <param name="rawValue">The raw value to parse.</param>
    private ISemanticVersion? GetSemanticVersion(string? rawValue)
    {
        return SemanticVersion.TryParse(rawValue, out ISemanticVersion? version)
            ? version
            : null;
    }
}
