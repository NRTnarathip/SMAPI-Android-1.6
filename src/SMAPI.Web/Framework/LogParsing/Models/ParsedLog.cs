using System;
using System.Diagnostics.CodeAnalysis;

namespace StardewModdingAPI.Web.Framework.LogParsing.Models;

/// <summary>Parsed metadata for a log.</summary>
public class ParsedLog
{
    /*********
    ** Accessors
    *********/
    /****
    ** Metadata
    ****/
    /// <summary>Whether the log file was successfully parsed.</summary>
    [MemberNotNullWhen(true, nameof(ParsedLog.RawText))]
    public bool IsValid { get; set; }

    /// <summary>An error message indicating why the log file is invalid.</summary>
    public string? Error { get; set; }

    /// <summary>The raw log text.</summary>
    public string? RawText { get; set; }

    /// <summary>Whether there are messages from multiple screens in the log.</summary>
    public bool IsSplitScreen { get; set; }

    /****
    ** Log data
    ****/
    /// <summary>The SMAPI version.</summary>
    public string? ApiVersion { get; set; }

    /// <summary>The game version.</summary>
    public string? GameVersion { get; set; }

    /// <summary>The player's operating system.</summary>
    public string? OperatingSystem { get; set; }

    /// <summary>The game install path.</summary>
    public string? GamePath { get; set; }

    /// <summary>The mod folder path.</summary>
    public string? ModPath { get; set; }

    /// <summary>The ISO 8601 timestamp when the log was started.</summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>Metadata about installed mods and content packs.</summary>
    public LogModInfo[] Mods { get; set; } = [];

    /// <summary>The log messages.</summary>
    public LogMessage[] Messages { get; set; } = [];

    /// <summary>The number of loaded non-content-pack mods.</summary>
    public int TotalCodeMods { get; set; }

    /// <summary>The number of loaded content packs.</summary>
    public int TotalContentPacks { get; set; }

    /// <summary>Whether update alerts were detected for SMAPI itself.</summary>
    public bool HasApiUpdate { get; set; }

    /// <summary>Whether update alerts were detected for one or more installed mods.</summary>
    public bool HasModUpdates { get; set; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an empty instance.</summary>
    public ParsedLog() { }

    /// <summary>Construct an instance.</summary>
    /// <param name="log">The other log instance to copy.</param>
    public ParsedLog(ParsedLog log)
    {
        // metadata
        this.IsValid = log.IsValid;
        this.Error = log.Error;
        this.RawText = log.RawText;
        this.IsSplitScreen = log.IsSplitScreen;

        // log data
        this.ApiVersion = log.ApiVersion;
        this.GameVersion = log.GameVersion;
        this.OperatingSystem = log.OperatingSystem;
        this.GamePath = log.GamePath;
        this.ModPath = log.ModPath;
        this.Timestamp = log.Timestamp;
        this.Mods = log.Mods;
        this.Messages = log.Messages;
        this.TotalCodeMods = log.TotalCodeMods;
        this.TotalContentPacks = log.TotalContentPacks;
        this.HasApiUpdate = log.HasApiUpdate;
        this.HasModUpdates = log.HasModUpdates;
    }
}
