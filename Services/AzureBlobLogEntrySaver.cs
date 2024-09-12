using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using LoggingWebApi.Configuration;
using LoggingWebApi.Interfaces;
using LoggingWebApi.Models;
using Microsoft.Extensions.Options;

namespace LoggingWebApi.Services
{
    /// <summary>
    /// Provides functionality to save log entries, including any associated binary attachments, to Azure Blob Storage.
    /// </summary>
    public class AzureBlobLogEntrySaver : ILogEntrySaver
    {
        private readonly LoggingOptions _options;
        private readonly BlobServiceClient _blobServiceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobLogEntrySaver"/> class with the specified logging options.
        /// </summary>
        /// <param name="options">The logging options configuration provided via dependency injection.</param>
        public AzureBlobLogEntrySaver(IOptions<LoggingOptions> options)
        {
            _options = options.Value;
            _blobServiceClient = new BlobServiceClient(_options.AzureBlobConnectionString);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobLogEntrySaver"/> class with the specified logging options.
        /// </summary>
        /// <param name="options">The logging options configuration.</param>
        public AzureBlobLogEntrySaver(LoggingOptions options)
        {
            _options = options;
            _blobServiceClient = new BlobServiceClient(_options.AzureBlobConnectionString);
        }

        /// <summary>
        /// Saves a <see cref="LogEntry"/> to Azure Blob Storage and any associated binary attachments.
        /// </summary>
        /// <param name="logEntry">The log entry to save.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public async Task SaveLogEntry(LogEntry logEntry)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_options.AzureBlobContainerName);
            await containerClient.CreateIfNotExistsAsync();

            // Save the log entry as a text file
            string logFileName = logEntry.GetLogFilename();
            var logBlobClient = containerClient.GetBlobClient(logFileName);
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(logEntry.ToString())))
            {
                await logBlobClient.UploadAsync(stream, overwrite: true);
            }

            // Check if the log entry has a binary attachment
            if (logEntry.HasBinaryAttachment)
            {
                // Ensure a valid filename is set for the binary attachment
                if (string.IsNullOrWhiteSpace(logEntry.BinaryAttachmentFilename))
                {
                    logEntry.BinaryAttachmentFilename = logEntry.GetBinaryAttachmentFilename();
                }

                // Save the binary attachment to a blob
                await SaveBinaryAttachmentAsync(logEntry.BinaryAttachment!, logEntry.BinaryAttachmentFilename, containerClient);
            }
        }

        /// <summary>
        /// Saves a binary attachment to Azure Blob Storage.
        /// </summary>
        /// <param name="bodyStream">The stream containing the binary content.</param>
        /// <param name="filename">The name of the blob to save the binary content to.</param>
        /// <param name="containerClient">The BlobContainerClient to use for saving the blob.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        private async Task SaveBinaryAttachmentAsync(Stream bodyStream, string filename, BlobContainerClient containerClient)
        {
            var blobClient = containerClient.GetBlobClient(filename);
            await blobClient.UploadAsync(bodyStream, overwrite: true);
        }
    }
}