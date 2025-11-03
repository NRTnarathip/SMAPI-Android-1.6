using System.Diagnostics.CodeAnalysis;

namespace StardewModdingAPI.Web.Framework.Storage;

/// <summary>The result of an attempt to upload a file.</summary>
internal class UploadResult
{
    /*********
    ** Accessors
    *********/
    /// <summary>Whether the file upload succeeded.</summary>
    [MemberNotNullWhen(true, nameof(UploadResult.Id))]
    [MemberNotNullWhen(false, nameof(UploadResult.UploadError))]
    public bool Succeeded => this.Id != null && this.UploadError == null;

    /// <summary>The file ID, if applicable.</summary>
    public string? Id { get; }

    /// <summary>The upload error, if any.</summary>
    public string? UploadError { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="id">The file ID, if applicable.</param>
    /// <param name="uploadError">The upload error, if any.</param>
    public UploadResult(string? id, string? uploadError)
    {
        this.Id = id;
        this.UploadError = uploadError;
    }
}
