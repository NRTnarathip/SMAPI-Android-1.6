using System.Threading.Tasks;

namespace StardewModdingAPI.Web.Framework.Storage;

/// <summary>Provides access to raw data storage.</summary>
internal interface IStorageProvider
{
    /// <summary>Save a text file to storage.</summary>
    /// <param name="id">The ID with which to save the result.</param>
    /// <param name="content">The content to upload.</param>
    /// <param name="contentType">The MIME content type being uploaded.</param>
    /// <param name="compress">Whether to gzip the text.</param>
    /// <returns>Returns metadata about the save attempt.</returns>
    Task<UploadResult> SaveAsync(string id, string content, string contentType, bool compress = true);

    /// <summary>Serialize a data model to JSON and save it to storage.</summary>
    /// <typeparam name="T">The data type to save.</typeparam>
    /// <param name="id">The ID with which to save the result.</param>
    /// <param name="content">The content to serialize and upload.</param>
    /// <param name="compress">Whether to gzip the text.</param>
    /// <returns>Returns metadata about the save attempt.</returns>
    Task<UploadResult> SaveToJsonAsync<T>(string id, T content, bool compress = true);

    /// <summary>Fetch raw text from storage.</summary>
    /// <param name="id">The storage ID used to upload the file.</param>
    /// <param name="forceRenew">Whether to reset the file expiry.</param>
    /// <param name="forceDownloadContent">Whether to download the file content even if a fetch URI can be provided.</param>
    Task<StoredFileInfo> GetAsync(string id, bool forceRenew, bool forceDownloadContent = false);
}
