using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StardewModdingAPI.Web.Framework.Clients.Pastebin;
using StardewModdingAPI.Web.Framework.Compression;
using StardewModdingAPI.Web.Framework.ConfigModels;

namespace StardewModdingAPI.Web.Framework.Storage;

/// <summary>Provides access to raw data storage.</summary>
internal class StorageProvider : IStorageProvider
{
    /*********
    ** Fields
    *********/
    /// <summary>The API client settings.</summary>
    private readonly ApiClientsConfig ClientsConfig;

    /// <summary>The underlying Pastebin client.</summary>
    private readonly IPastebinClient Pastebin;

    /// <summary>The underlying text compression helper.</summary>
    private readonly IGzipHelper GzipHelper;

    /// <summary>Whether Azure blob storage is configured.</summary>
    private bool HasAzure => !string.IsNullOrWhiteSpace(this.ClientsConfig.AzureBlobConnectionString);

    /// <inheritdoc cref="ApiClientsConfig.AzureBlobTempExpiryDays"/>
    private int ExpiryDays => this.ClientsConfig.AzureBlobTempExpiryDays;

    /// <inheritdoc cref="ApiClientsConfig.AzureBlobTempExpiryAutoRenewalDays"/>
    private int AutoRenewalDays => this.ClientsConfig.AzureBlobTempExpiryAutoRenewalDays;

    /// <summary>The JSON serializer settings to use when saving JSON data to storage.</summary>
    private JsonSerializerSettings DefaultJsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore
    };


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="clientsConfig">The API client settings.</param>
    /// <param name="pastebin">The underlying Pastebin client.</param>
    /// <param name="gzipHelper">The underlying text compression helper.</param>
    public StorageProvider(IOptions<ApiClientsConfig> clientsConfig, IPastebinClient pastebin, IGzipHelper gzipHelper)
    {
        this.ClientsConfig = clientsConfig.Value;
        this.Pastebin = pastebin;
        this.GzipHelper = gzipHelper;
    }

    /// <inheritdoc />
    public async Task<UploadResult> SaveAsync(string id, string content, string contentType, bool compress = true)
    {
        // save to Azure
        if (this.HasAzure)
        {
            try
            {
                await using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                BlobClient blob = this.GetAzureBlobClient(id);
                await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });

                return new UploadResult(id, null);
            }
            catch (Exception ex)
            {
                return new UploadResult(null, ex.Message);
            }
        }

        // save to local filesystem for testing
        else
        {
            string path = this.GetDevFilePath(id);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            await File.WriteAllTextAsync(path, content);
            return new UploadResult(id, null);
        }
    }

    /// <inheritdoc />
    public Task<UploadResult> SaveToJsonAsync<T>(string id, T content, bool compress = true)
    {
        string json = JsonConvert.SerializeObject(content, this.DefaultJsonSettings);

        return this.SaveAsync(id, json, "application/json", compress);
    }

    /// <inheritdoc />
    public async Task<StoredFileInfo> GetAsync(string id, bool forceRenew, bool forceDownloadContent = false)
    {
        // fetch from blob storage
        bool isBlobStorage = id.StartsWith("parsed-") || id == "mod-list" || Guid.TryParseExact(id, "N", out _);
        if (isBlobStorage)
        {
            // Azure Blob storage
            if (this.HasAzure)
            {
                try
                {
                    // fetch metadata
                    BlobClient blobClient = this.GetAzureBlobClient(id);
                    Response<BlobProperties> properties = await blobClient.GetPropertiesAsync();
                    DateTimeOffset lastModified = properties.Value.LastModified;
                    DateTimeOffset oldExpiry = this.GetExpiry(lastModified);

                    // get content or URL file
                    string? content = null;
                    string? fetchUri = null;
                    if (forceDownloadContent)
                    {
                        Response<BlobDownloadInfo> response = await blobClient.DownloadAsync();
                        using StreamReader reader = new(response.Value.Content);
                        content = this.GzipHelper.DecompressString(await reader.ReadToEndAsync());
                    }
                    else
                        fetchUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(5)).ToString();

                    // extend expiry if needed
                    DateTimeOffset newExpiry = oldExpiry;
                    if (forceRenew || this.IsWithinAutoRenewalWindow(lastModified))
                    {
                        await blobClient.SetMetadataAsync(new Dictionary<string, string> { ["expiryRenewed"] = DateTime.UtcNow.ToString("O") }); // change the blob's last-modified date (the specific property set doesn't matter)
                        newExpiry = this.GetExpiry(DateTimeOffset.UtcNow);
                    }

                    // build model
                    return new StoredFileInfo(fetchUri, content, oldExpiry, newExpiry);
                }
                catch (RequestFailedException ex)
                {
                    return new StoredFileInfo(
                        error: ex.ErrorCode == "BlobNotFound"
                            ? "There's no file with that ID."
                            : $"Could not fetch that file from storage ({ex.ErrorCode}: {ex.Message})."
                    );
                }
            }

            // local filesystem for testing
            else
            {
                // get file
                FileInfo file = new(this.GetDevFilePath(id));
                if (file.Exists && file.LastWriteTimeUtc.AddDays(this.ExpiryDays) < DateTime.UtcNow) // expired
                    file.Delete();
                if (!file.Exists)
                    return new StoredFileInfo(error: "There's no file with that ID.");

                // renew
                if (forceRenew)
                {
                    File.SetLastWriteTimeUtc(file.FullName, DateTime.UtcNow);
                    file.Refresh();
                }

                // build model
                return new StoredFileInfo(
                    fetchUri: null,
                    fetchedData: await File.ReadAllTextAsync(file.FullName),
                    oldExpiry: null,
                    newExpiry: DateTime.UtcNow.AddDays(this.ExpiryDays),
                    warning: "This file was saved temporarily to the local computer. This should only happen in a local development environment."
                );
            }
        }

        // get from Pastebin
        else
        {
            PasteInfo response = await this.Pastebin.GetAsync(id);
            response.Content = this.GzipHelper.DecompressString(response.Content);
            return new StoredFileInfo(null, response.Content, null, null, error: response.Error);
        }
    }

    /// <summary>Get the date when an uploaded file will expire.</summary>
    /// <param name="lastModified">The date when the file was uploaded or last renewed.</param>
    private DateTimeOffset GetExpiry(DateTimeOffset lastModified)
    {
        return lastModified + TimeSpan.FromDays(this.ExpiryDays);
    }

    /// <summary>Get whether a blob's expiry should be reset based on its last access date.</summary>
    /// <param name="lastModified">The date when the file was uploaded or last renewed.</param>
    private bool IsWithinAutoRenewalWindow(DateTimeOffset lastModified)
    {
        return lastModified + TimeSpan.FromDays(this.AutoRenewalDays) >= DateTimeOffset.UtcNow;
    }

    /// <summary>Get a client for reading and writing to Azure Blob storage.</summary>
    /// <param name="id">The file ID.</param>
    private BlobClient GetAzureBlobClient(string id)
    {
        BlobServiceClient azure = new(this.ClientsConfig.AzureBlobConnectionString);
        BlobContainerClient container = azure.GetBlobContainerClient(this.ClientsConfig.AzureBlobTempContainer);
        return container.GetBlobClient($"uploads/{id}");
    }

    /// <summary>Get the absolute file path for an upload when running in a local test environment with no Azure account configured.</summary>
    /// <param name="id">The file ID.</param>
    private string GetDevFilePath(string id)
    {
        return Path.Combine(Path.GetTempPath(), "smapi-web-temp", $"{id}.txt");
    }
}
