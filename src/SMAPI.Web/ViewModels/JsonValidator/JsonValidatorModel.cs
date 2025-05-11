using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewModdingAPI.Web.ViewModels.JsonValidator;

/// <summary>The view model for the JSON validator page.</summary>
public class JsonValidatorModel
{
    /*********
    ** Accessors
    *********/
    /// <summary>Whether to show the edit view.</summary>
    public bool IsEditView { get; }

    /// <summary>The paste ID.</summary>
    public string? PasteId { get; }

    /// <summary>The schema name with which the JSON was validated.</summary>
    public string? SchemaName { get; }

    /// <summary>The supported JSON schemas (names indexed by ID).</summary>
    public IDictionary<string, string> SchemaFormats { get; }

    /// <summary>The validated content.</summary>
    public string? Content { get; set; }

    /// <summary>The schema validation errors, if any.</summary>
    public JsonValidatorErrorModel[] Errors { get; set; } = [];

    /// <summary>A non-blocking warning while uploading the file.</summary>
    public string? UploadWarning { get; set; }

    /// <summary>When the uploaded file would no longer have been available, before any renewal applied in this request.</summary>
    public DateTimeOffset? OldExpiry { get; set; }

    /// <summary>When the file will no longer be available, after any renewal applied in this request.</summary>
    public DateTimeOffset? NewExpiry { get; set; }

    /// <summary>An error which occurred while uploading the JSON.</summary>
    public string? UploadError { get; set; }

    /// <summary>An error which occurred while parsing the JSON.</summary>
    public string? ParseError { get; set; }

    /// <summary>A web URL to the user-facing format documentation.</summary>
    public string? FormatUrl { get; set; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="pasteId">The stored file ID.</param>
    /// <param name="schemaName">The schema name with which the JSON was validated.</param>
    /// <param name="schemaFormats">The supported JSON schemas (names indexed by ID).</param>
    /// <param name="isEditView">Whether to show the edit view.</param>
    public JsonValidatorModel(string? pasteId, string? schemaName, IDictionary<string, string> schemaFormats, bool isEditView)
    {
        this.PasteId = pasteId;
        this.SchemaName = schemaName;
        this.SchemaFormats = schemaFormats;
        this.IsEditView = isEditView;
    }

    /// <summary>Set the validated content.</summary>
    /// <param name="content">The validated content.</param>
    /// <param name="oldExpiry">When the uploaded file would no longer have been available, before any renewal applied in this request.</param>
    /// <param name="newExpiry">When the file will no longer be available, after any renewal applied in this request.</param>
    /// <param name="uploadWarning">A non-blocking warning while uploading the log.</param>
    public JsonValidatorModel SetContent(string content, DateTimeOffset? oldExpiry, DateTimeOffset? newExpiry, string? uploadWarning = null)
    {
        this.Content = content;
        this.OldExpiry = oldExpiry;
        this.UploadWarning = uploadWarning;

        return this;
    }

    /// <summary>Set the error which occurred while uploading the JSON.</summary>
    /// <param name="error">The error message.</param>
    public JsonValidatorModel SetUploadError(string error)
    {
        this.UploadError = error;

        return this;
    }

    /// <summary>Set the error which occurred while parsing the JSON.</summary>
    /// <param name="error">The error message.</param>
    public JsonValidatorModel SetParseError(string error)
    {
        this.ParseError = error;

        return this;
    }

    /// <summary>Add validation errors to the response.</summary>
    /// <param name="errors">The schema validation errors.</param>
    public JsonValidatorModel AddErrors(params JsonValidatorErrorModel[] errors)
    {
        this.Errors = this.Errors.Concat(errors).ToArray();

        return this;
    }
}
