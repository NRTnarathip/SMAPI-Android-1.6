using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using StardewModdingAPI.Toolkit.Utilities;
using StardewModdingAPI.Web.Framework.LogParsing.Models;

namespace StardewModdingAPI.Web.ViewModels;

/// <summary>The view model for the log parser page.</summary>
public class LogParserModel
{
    /*********
    ** Accessors
    *********/
    /// <summary>The paste ID.</summary>
    public string? PasteId { get; }

    /// <summary>The viewer's detected OS, if known.</summary>
    public Platform? DetectedPlatform { get; }

    /// <summary>The URI from which to fetch the log data.</summary>
    public string? FetchUri { get; private set; }

    /// <summary>The pre-fetched data, if a <see cref="FetchUri"/> can't be provided for this file.</summary>
    public ParsedLog? FetchedData { get; private set; }

    /// <summary>Whether to show the raw unparsed log.</summary>
    public bool ShowRaw { get; private set; }

    /// <summary>A non-blocking warning while uploading the log.</summary>
    public string? UploadWarning { get; set; }

    /// <summary>An error which occurred while uploading the log.</summary>
    public string? UploadError { get; set; }

    /// <summary>When the uploaded file would no longer have been available, before any renewal applied in this request.</summary>
    public DateTimeOffset? OldExpiry { get; set; }

    /// <summary>When the file will no longer be available, after any renewal applied in this request.</summary>
    public DateTimeOffset? NewExpiry { get; set; }

    /// <summary>Whether parsed log data is available, regardless of whether it's valid.</summary>
    [MemberNotNullWhen(true, nameof(LogParserModel.PasteId))]
    public bool HasLog => this.FetchUri != null || this.FetchedData != null;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="pasteId">The paste ID.</param>
    /// <param name="platform">The viewer's detected OS, if known.</param>
    public LogParserModel(string? pasteId, Platform? platform)
    {
        this.PasteId = pasteId;
        this.DetectedPlatform = platform;
        this.ShowRaw = false;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="pasteId">The paste ID.</param>
    /// <param name="detectedPlatform">The viewer's detected OS, if known.</param>
    /// <param name="fetchUri">The URI from which to fetch the log data.</param>
    /// <param name="showRaw">Whether to show the raw unparsed log.</param>
    /// <param name="uploadWarning">A non-blocking warning while uploading the log.</param>
    /// <param name="uploadError">An error which occurred while uploading the log.</param>
    /// <param name="oldExpiry">When the uploaded file would no longer have been available, before any renewal applied in this request.</param>
    /// <param name="newExpiry">When the file will no longer be available, after any renewal applied in this request.</param>
    [JsonConstructor]
    public LogParserModel(string pasteId, Platform? detectedPlatform, string fetchUri, bool showRaw, string? uploadWarning, string? uploadError, DateTime? oldExpiry, DateTime? newExpiry)
    {
        this.PasteId = pasteId;
        this.DetectedPlatform = detectedPlatform;
        this.FetchUri = fetchUri;
        this.ShowRaw = showRaw;
        this.UploadWarning = uploadWarning;
        this.UploadError = uploadError;
        this.OldExpiry = oldExpiry;
        this.NewExpiry = newExpiry;
    }

    /// <summary>Set the log parser result.</summary>
    /// <param name="fetchUri">The URI from which to fetch the log data.</param>
    /// <param name="fetchedData">The pre-fetched data, if a <paramref name="fetchUri"/> can't be provided for this file.</param>
    /// <param name="showRaw">Whether to show the raw unparsed log.</param>
    public LogParserModel SetResult(string? fetchUri, ParsedLog? fetchedData, bool showRaw)
    {
        this.FetchUri = fetchUri;
        this.FetchedData = fetchedData;
        this.ShowRaw = showRaw;

        return this;
    }
}
