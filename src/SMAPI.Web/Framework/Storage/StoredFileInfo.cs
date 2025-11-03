using System;
using System.Diagnostics.CodeAnalysis;

namespace StardewModdingAPI.Web.Framework.Storage;

/// <summary>The response for a get-file request.</summary>
internal class StoredFileInfo
{
    /*********
    ** Accessors
    *********/
    /// <summary>Whether the file was successfully fetched.</summary>
    public bool Success => (this.FetchedData != null || this.FetchUri != null) && this.Error == null;

    /// <summary>The URI from which to fetch the file, if available.</summary>
    public string? FetchUri { get; }

    /// <summary>The pre-fetched data, if a <see name="FetchUri"/> can't be provided for this file.</summary>
    public string? FetchedData { get; }

    /// <summary>When the file would no longer have been available, before any renewal applied in this request.</summary>
    public DateTimeOffset? OldExpiry { get; }

    /// <summary>When the file will no longer be available, after any renewal applied in this request.</summary>
    public DateTimeOffset? NewExpiry { get; }

    /// <summary>The error message if saving succeeded, but a non-blocking issue was encountered.</summary>
    public string? Warning { get; }

    /// <summary>The error message if saving failed.</summary>
    public string? Error { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="fetchUri">The URI from which to fetch the file, if available.</param>
    /// <param name="fetchedData">The pre-fetched data, if a <paramref name="fetchUri"/> can't be provided for this file.</param>
    /// <param name="oldExpiry">When the file would no longer have been available, before any renewal applied in this request.</param>
    /// <param name="newExpiry">When the file will no longer be available, after any renewal applied in this request.</param>
    /// <param name="warning">The error message if saving succeeded, but a non-blocking issue was encountered.</param>
    /// <param name="error">The error message if saving failed.</param>
    public StoredFileInfo(string? fetchUri, string? fetchedData, DateTimeOffset? oldExpiry, DateTimeOffset? newExpiry, string? warning = null, string? error = null)
    {
        this.FetchUri = fetchUri;
        this.FetchedData = fetchedData;
        this.OldExpiry = oldExpiry;
        this.NewExpiry = newExpiry;
        this.Warning = warning;
        this.Error = error;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="error">The error message if saving failed.</param>
    public StoredFileInfo(string error)
    {
        this.Error = error;
    }
}
