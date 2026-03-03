using System;
using System.IO;
using System.Threading.Tasks;

namespace KalolCommunity.Application.Interfaces
{
    public interface IBlobService
    {
        /// <summary>
        /// Upload temporary profile photo to Azure Blob Storage temp folder
        /// </summary>
        /// <param name="fileStream">Image file stream</param>
        /// <param name="fileName">Original file name with extension</param>
        /// <returns>Generated unique file name (GUID)</returns>
        Task<string> UploadTempAsync(Stream fileStream, string fileName);

        /// <summary>
        /// Move image from temp folder to permanent members folder
        /// </summary>
        /// <param name="tempFileName">File name in temp folder</param>
        /// <returns>Permanent blob URL or filename (implementation may return filename)</returns>
        Task<string> MoveToPermanentAsync(string tempFileName);

        /// <summary>
        /// Delete temporary file after failed registration
        /// </summary>
        /// <param name="tempFileName">File name in temp folder</param>
        Task DeleteTempAsync(string tempFileName);

        /// <summary>
        /// Get blob URL for a permanent member photo
        /// </summary>
        /// <param name="fileName">File name in members folder</param>
        /// <returns>Full blob URL</returns>
        string GetPermanentBlobUrl(string fileName);

        /// <summary>
        /// Generate a time-limited read-only SAS URL for a member photo.
        /// </summary>
        /// <param name="fileName">File name in members folder</param>
        /// <param name="validFor">Optional SAS validity duration</param>
        /// <returns>SAS URL string</returns>
        string GetPermanentBlobSasUrl(string fileName, TimeSpan? validFor = null);
    }
}
