using System;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI.Toolkit;

namespace StardewModdingAPI.Web.Framework.LogParsing.Models;

/// <summary>Metadata about a mod or content pack in the log.</summary>
public class LogModInfo
{
    /*********
    ** Private fields
    *********/
    /// <summary>The parsed mod version, if valid.</summary>
    private Lazy<ISemanticVersion?> ParsedVersionImpl;


    /*********
    ** Accessors
    *********/
    /// <summary>The mod name.</summary>
    public string Name { get; }

    /// <summary>The mod author.</summary>
    public string Author { get; }

    /// <summary>The mod version.</summary>
    public string? Version { get; private set; }

    /// <summary>The mod description.</summary>
    public string Description { get; }

    /// <summary>The update version.</summary>
    public string? UpdateVersion { get; private set; }

    /// <summary>The update link.</summary>
    public string? UpdateLink { get; private set; }

    /// <summary>The name of the mod for which this is a content pack (if applicable).</summary>
    public string? ContentPackFor { get; }

    /// <summary>The number of errors logged by this mod.</summary>
    public int Errors { get; set; }

    /// <summary>Whether the mod was loaded into the game.</summary>
    public bool Loaded { get; }

    /// <summary>Whether the mod has an update available.</summary>
    [MemberNotNullWhen(true, nameof(LogModInfo.UpdateVersion), nameof(LogModInfo.UpdateLink))]
    public bool HasUpdate => this.UpdateVersion != null && this.Version != this.UpdateVersion;

    /// <summary>The mod type.</summary>
    public ModType ModType { get; }

    /// <summary>Whether this is an actual mod (rather than a special entry for SMAPI or the game itself).</summary>
    public bool IsMod => this.ModType != ModType.Special;

    /// <summary>Whether this is a C# code mod.</summary>
    public bool IsCodeMod => this.ModType == ModType.CodeMod;

    /// <summary>Whether this is a content pack for another mod.</summary>
    public bool IsContentPack => this.ModType == ModType.ContentPack;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modType">The mod type.</param>
    /// <param name="name">The mod name.</param>
    /// <param name="author">The mod author.</param>
    /// <param name="version">The mod version.</param>
    /// <param name="description">The mod description.</param>
    /// <param name="updateVersion">The update version.</param>
    /// <param name="updateLink">The update link.</param>
    /// <param name="contentPackFor">The name of the mod for which this is a content pack (if applicable).</param>
    /// <param name="errors">The number of errors logged by this mod.</param>
    /// <param name="loaded">Whether the mod was loaded into the game.</param>
    public LogModInfo(ModType modType, string name, string author, string version, string description, string? updateVersion = null, string? updateLink = null, string? contentPackFor = null, int errors = 0, bool loaded = true)
    {
        this.ModType = modType;
        this.Name = name;
        this.Author = author;
        this.Description = description;
        this.UpdateVersion = updateVersion;
        this.UpdateLink = updateLink;
        this.ContentPackFor = contentPackFor;
        this.Errors = errors;
        this.Loaded = loaded;

        this.OverrideVersion(version);
    }

    /// <summary>Add an update alert for this mod.</summary>
    /// <param name="updateVersion">The update version.</param>
    /// <param name="updateLink">The update link.</param>
    public void SetUpdate(string updateVersion, string updateLink)
    {
        this.UpdateVersion = updateVersion;
        this.UpdateLink = updateLink;
    }

    /// <summary>Override the version number, for cases like SMAPI itself where the version is only known later during parsing.</summary>
    /// <param name="version">The new mod version.</param>
    [MemberNotNull(nameof(LogModInfo.Version), nameof(LogModInfo.ParsedVersionImpl))]
    public void OverrideVersion(string version)
    {
        this.Version = version;
        this.ParsedVersionImpl = new Lazy<ISemanticVersion?>(this.ParseVersion);
    }

    /// <summary>Get the semantic version for this mod, if it's valid.</summary>
    public ISemanticVersion? GetParsedVersion()
    {
        return this.ParsedVersionImpl.Value;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the semantic version for this mod, if it's valid.</summary>
    public ISemanticVersion? ParseVersion()
    {
        return !string.IsNullOrWhiteSpace(this.Version) && SemanticVersion.TryParse(this.Version, out ISemanticVersion? version)
            ? version
            : null;
    }
}
