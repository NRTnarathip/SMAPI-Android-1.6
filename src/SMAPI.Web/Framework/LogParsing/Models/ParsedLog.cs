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
    /// <summary>Whether SMAPI is running in strict mode, which disables all deprecated APIs.</summary>
    public bool IsStrictMode { get; set; }

    /// <summary>The SMAPI version.</summary>
    public string? ApiVersion { get; set; }

    /// <summary>The parsed SMAPI version, if it's valid.</summary>
    public ISemanticVersion? ApiVersionParsed { get; set; }

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
}
