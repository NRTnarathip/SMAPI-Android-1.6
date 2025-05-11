using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Newtonsoft.Json;
using StardewModdingAPI.Toolkit.Serialization.Converters;

namespace StardewModdingAPI.Toolkit.Serialization.Models;

/// <inheritdoc cref="IManifest" />
public class Manifest : IManifest
{
    /*********
    ** Accessors
    *********/
    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public string Author { get; }

    /// <inheritdoc />
    public ISemanticVersion Version { get; }

    /// <inheritdoc />
    public ISemanticVersion? MinimumApiVersion { get; }

    /// <inheritdoc />
    public ISemanticVersion? MinimumGameVersion { get; }

    /// <inheritdoc />
    public string? EntryDll { get; }

    /// <inheritdoc />
    [JsonConverter(typeof(ManifestContentPackForConverter))]
    public IManifestContentPackFor? ContentPackFor { get; }

    /// <inheritdoc />
    [JsonConverter(typeof(ManifestDependencyArrayConverter))]
    public IManifestDependency[] Dependencies { get; }

    /// <inheritdoc />
    public string[] UpdateKeys { get; private set; }

    /// <inheritdoc />
    public string UniqueID { get; }

    /// <inheritdoc />
    [JsonExtensionData]
    public IDictionary<string, object> ExtraFields { get; } = new Dictionary<string, object>();


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance for a transitional content pack.</summary>
    /// <param name="uniqueId">The unique mod ID.</param>
    /// <param name="name">The mod name.</param>
    /// <param name="author">The mod author's name.</param>
    /// <param name="description">A brief description of the mod.</param>
    /// <param name="version">The mod version.</param>
    /// <param name="contentPackFor">The mod ID which will read this as a content pack.</param>
    public Manifest(string uniqueId, string name, string author, string description, ISemanticVersion version, string? contentPackFor = null)
        : this(
            uniqueId: uniqueId,
            name: name,
            author: author,
            description: description,
            version: version,
            minimumApiVersion: null,
            minimumGameVersion: null,
            entryDll: null,
            contentPackFor: contentPackFor != null
                ? new ManifestContentPackFor(contentPackFor, null)
                : null,
            dependencies: null,
            updateKeys: null
        )
    { }

    /// <summary>Construct an instance for a transitional content pack.</summary>
    /// <param name="uniqueId">The unique mod ID.</param>
    /// <param name="name">The mod name.</param>
    /// <param name="author">The mod author's name.</param>
    /// <param name="description">A brief description of the mod.</param>
    /// <param name="version">The mod version.</param>
    /// <param name="minimumApiVersion">The minimum SMAPI version required by this mod, if any.</param>
    /// <param name="minimumGameVersion">The minimum Stardew Valley version required by this mod, if any.</param>
    /// <param name="entryDll">The name of the DLL in the directory that has the <c>Entry</c> method. Mutually exclusive with <see cref="ContentPackFor"/>.</param>
    /// <param name="contentPackFor">The mod ID which will read this as a content pack.</param>
    /// <param name="dependencies">The other mods that must be loaded before this mod.</param>
    /// <param name="updateKeys">The namespaced mod IDs to query for updates (like <c>Nexus:541</c>).</param>
    [JsonConstructor]
    public Manifest(string uniqueId, string name, string author, string description, ISemanticVersion version, ISemanticVersion? minimumApiVersion, ISemanticVersion? minimumGameVersion, string? entryDll, IManifestContentPackFor? contentPackFor, IManifestDependency[]? dependencies, string[]? updateKeys)
    {
        this.UniqueID = this.NormalizeField(uniqueId);
        this.Name = this.NormalizeField(name, replaceSquareBrackets: true);
        this.Author = this.NormalizeField(author);
        this.Description = this.NormalizeField(description);
        this.Version = version;
        this.MinimumApiVersion = minimumApiVersion;
        this.MinimumGameVersion = minimumGameVersion;
        this.EntryDll = this.NormalizeField(entryDll);
        this.ContentPackFor = contentPackFor;
        this.Dependencies = dependencies ?? [];
        this.UpdateKeys = updateKeys ?? [];
    }

    /// <summary>Override the update keys loaded from the mod info.</summary>
    /// <param name="updateKeys">The new update keys to set.</param>
    internal void OverrideUpdateKeys(params string[] updateKeys)
    {
        this.UpdateKeys = updateKeys;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Normalize a manifest field to strip newlines, trim whitespace, and optionally strip square brackets.</summary>
    /// <param name="input">The input to strip.</param>
    /// <param name="replaceSquareBrackets">Whether to replace square brackets with round ones. This is used in the mod name to avoid breaking the log format.</param>
#if NET6_0_OR_GREATER
    [return: NotNullIfNotNull("input")]
#endif
    private string? NormalizeField(string? input, bool replaceSquareBrackets = false)
    {
        input = input?.Trim();

        if (!string.IsNullOrEmpty(input))
        {
            StringBuilder? builder = null;

            for (int i = 0; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case '\r':
                    case '\n':
                        builder ??= new StringBuilder(input);
                        builder[i] = ' ';
                        break;

                    case '[' when replaceSquareBrackets:
                        builder ??= new StringBuilder(input);
                        builder[i] = '(';
                        break;

                    case ']' when replaceSquareBrackets:
                        builder ??= new StringBuilder(input);
                        builder[i] = ')';
                        break;
                }
            }

            if (builder != null)
                input = builder.ToString();
        }

        return input;
    }

    /// <summary>Normalize whitespace in a raw string.</summary>
    /// <param name="input">The input to strip.</param>
#if NET6_0_OR_GREATER
    [return: NotNullIfNotNull("input")]
#endif
    internal static string? NormalizeWhitespace(string? input)
    {
        return input?.Trim();
    }
}
