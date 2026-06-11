using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using KalolCommunity.Application.Interfaces.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KalolCommunity.Infrastructure.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly string _connectionString;
        private readonly ILogger<BlobService> _logger;
        private const string TempFolder = "temp";
        private const string MembersFolder = "members";
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

        public BlobService(IConfiguration configuration, ILogger<BlobService> logger)
        {
            _logger = logger;

            // Preferred values
            var preferredConnectionString = configuration["AzureBlobStorage:ConnectionString"];
            var preferredContainerName = configuration["AzureBlobStorage:ContainerName"];

            // Fallback values
            var fallbackConnectionString = configuration["BlobStorage:Storage"];
            var fallbackContainerName = configuration["BlobStorage:Container"];

            // ✅ Proper fallback (handles null + empty)
            var connectionString = string.IsNullOrWhiteSpace(preferredConnectionString)
                ? fallbackConnectionString
                : preferredConnectionString;

            var containerName = string.IsNullOrWhiteSpace(preferredContainerName)
                ? fallbackContainerName
                : preferredContainerName;

            // ✅ Minimal useful logging (no secrets exposed)
            _logger.LogInformation("[BlobService] Using ConnectionString from: {ConfigSource}",
                !string.IsNullOrWhiteSpace(preferredConnectionString) ? "AzureBlobStorage" : "BlobStorage");

            _logger.LogInformation("[BlobService] Container Name: {ContainerName}", containerName);

            // avoid logging full secrets; length is acceptable for diagnostics
            _logger.LogInformation("[BlobService] ConnectionString Length: {Length}", connectionString?.Length);

            // ✅ Validations
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "Blob storage connection string is missing.");

            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentNullException(nameof(containerName), "Blob container name is missing.");

            _connectionString = connectionString;

            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            _logger.LogInformation("[BlobService] Initialized successfully.");
        }

        /// <summary>
        /// Upload temporary profile photo to temp folder
        /// </summary>
        public async Task<string> UploadTempAsync(Stream fileStream, string fileName)
        {
            // Validate file
            ValidateFile(fileStream, fileName);

            // Generate unique file name
            string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            string blobName = $"{TempFolder}/{uniqueFileName}";

            // Reset stream position
            fileStream.Position = 0;

            // Upload to Azure Blob Storage
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(fileStream, overwrite: true);

            _logger.LogInformation("Temporary file uploaded: {BlobName}", blobName);

            return uniqueFileName;
        }

        /// <summary>
        /// Move image from temp folder to permanent members folder
        /// </summary>
        public async Task<string> MoveToPermanentAsync(string tempFileName)
        {
            string tempBlobName = $"{TempFolder}/{tempFileName}";
            string permanentBlobName = $"{MembersFolder}/{tempFileName}";

            BlobClient tempBlobClient = _containerClient.GetBlobClient(tempBlobName);

            if (!await tempBlobClient.ExistsAsync())
            {
                throw new FileNotFoundException($"Temporary file not found: {tempFileName}");
            }

            BlobClient permanentBlobClient = _containerClient.GetBlobClient(permanentBlobName);

            await permanentBlobClient.StartCopyFromUriAsync(tempBlobClient.Uri);

            await WaitForCopyCompleteAsync(permanentBlobClient);

            await tempBlobClient.DeleteAsync();

            _logger.LogInformation("File moved from {TempBlob} to {PermanentBlob}", tempBlobName, permanentBlobName);

            // Return filename only so callers store the filename in DB
            return tempFileName;
        }

        /// <summary>
        /// Delete temporary file (for failed registrations)
        /// </summary>
        public async Task DeleteTempAsync(string tempFileName)
        {
            string blobName = $"{TempFolder}/{tempFileName}";
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteAsync();
                _logger.LogInformation("Temporary file deleted: {BlobName}", blobName);
            }
        }

        /// <summary>
        /// Get full blob URL for permanent member photo (no SAS). If storage account disables public access,
        /// the returned URL will not be directly usable in a browser.
        /// </summary>
        public string GetPermanentBlobUrl(string fileName)
        {
            string blobName = $"{MembersFolder}/{fileName}";
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);
            return blobClient.Uri.ToString();
        }

        /// <summary>
        /// Generate a time-limited read-only SAS URL for a member photo.
        /// Use this when the storage account or container does not allow anonymous public access.
        /// </summary>
        public string GetPermanentBlobSasUrl(string fileName, TimeSpan? validFor = null)
        {
            string blobName = $"{MembersFolder}/{fileName}";
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);

            // Default SAS validity
            var expiresOn = DateTimeOffset.UtcNow.Add(validFor ?? TimeSpan.FromHours(1));

            // Extract AccountName and AccountKey from connection string (required for StorageSharedKeyCredential)
            string? accountName = GetConnectionStringValue(_connectionString, "AccountName");
            string? accountKey = GetConnectionStringValue(_connectionString, "AccountKey");

            if (string.IsNullOrWhiteSpace(accountName) || string.IsNullOrWhiteSpace(accountKey))
            {
                throw new InvalidOperationException("Storage account key is required to generate SAS. Consider using Azure AD user delegation SAS or enable container public access.");
            }

            var sharedKeyCredential = new StorageSharedKeyCredential(accountName!, accountKey!);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerClient.Name,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = expiresOn
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasQueryParameters = sasBuilder.ToSasQueryParameters(sharedKeyCredential).ToString();

            var uriBuilder = new UriBuilder(blobClient.Uri) { Query = sasQueryParameters };
            return uriBuilder.Uri.ToString();
        }

        /// <summary>
        /// Validate file size and type
        /// </summary>
        private static void ValidateFile(Stream fileStream, string fileName)
        {
            // Check file size
            if (fileStream.Length > MaxFileSize)
            {
                throw new InvalidOperationException($"File size exceeds maximum allowed size of 2MB. Current size: {fileStream.Length / (1024 * 1024)}MB");
            }

            // Check file extension
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (Array.IndexOf(AllowedExtensions, extension) < 0)
            {
                throw new InvalidOperationException($"File type '{extension}' is not allowed. Allowed types: jpg, jpeg, png");
            }
        }

        /// <summary>
        /// Wait for blob copy operation to complete
        /// </summary>
        private static async Task WaitForCopyCompleteAsync(BlobClient blobClient, int maxWaitSeconds = 30)
        {
            var startTime = DateTime.UtcNow;
            var timeout = TimeSpan.FromSeconds(maxWaitSeconds);

            while (DateTime.UtcNow - startTime < timeout)
            {
                BlobProperties properties = await blobClient.GetPropertiesAsync();

                // Use the CopyStatus enum directly, as BlobCopyStatus is a nullable CopyStatus property
                if (properties.BlobCopyStatus == CopyStatus.Success)
                {
                    return;
                }

                if (properties.BlobCopyStatus == CopyStatus.Failed ||
                    properties.BlobCopyStatus == CopyStatus.Aborted)
                {
                    throw new InvalidOperationException($"Blob copy failed with status: {properties.BlobCopyStatus}");
                }

                await Task.Delay(1000);
            }

            throw new TimeoutException($"Blob copy operation timed out after {maxWaitSeconds} seconds");
        }

        private static string? GetConnectionStringValue(string connectionString, string key)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return null;
            }

            var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var kv = part.Split('=', 2);
                if (kv.Length == 2 && kv[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    return kv[1];
                }
            }

            return null;
        }
    }
}