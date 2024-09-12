using System.IO;
using System.Threading.Tasks;
using LoggingWebApi.Configuration;
using LoggingWebApi.Interfaces;
using LoggingWebApi.Models;
using Microsoft.Extensions.Options;

namespace LoggingWebApi.Services
{
    /// <summary>
    /// Provides functionality to save log entries, including any associated binary attachments, to a file system.
    /// </summary>
    public class FileSystemLogEntrySaver : ILogEntrySaver
    {
        /// <summary>
        /// Configuration options for logging, such as the log file directory.
        /// </summary>
        private readonly LoggingOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemLogEntrySaver"/> class with the specified logging options.
        /// </summary>
        /// <param name="options">The logging options configuration provided via dependency injection.</param>
        public FileSystemLogEntrySaver(IOptions<LoggingOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemLogEntrySaver"/> class with the specified logging options.
        /// </summary>
        /// <param name="options">The logging options configuration.</param>
        public FileSystemLogEntrySaver(LoggingOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Saves a <see cref="LogEntry"/> to a file and any associated binary attachments.
        /// </summary>
        /// <param name="logEntry">The log entry to save.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public async Task SaveLogEntry(LogEntry logEntry)
        {
            // Check if the log entry has a binary attachment
            if (logEntry.HasBinaryAttachment)
            {
                // Ensure a valid filename is set for the binary attachment
                if (string.IsNullOrWhiteSpace(logEntry.BinaryAttachmentFilename))
                {
                    logEntry.BinaryAttachmentFilename = logEntry.GetBinaryAttachmentFilename();
                }

                // Save the binary attachment to a file
                await SaveBinaryAttachmentAsync(logEntry.BinaryAttachment!, logEntry.BinaryAttachmentFilename);
            }

            // Construct the file path for saving the log entry
            string logFilePath = Path.Combine(_options.LogFileDirectory, logEntry.GetLogFilename());

            // Save the log entry as a text file
            await File.WriteAllTextAsync(logFilePath, logEntry.ToString());
        }

        /// <summary>
        /// Saves a binary attachment to a file.
        /// </summary>
        /// <param name="bodyStream">The stream containing the binary content.</param>
        /// <param name="filename">The name of the file to save the binary content to.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        private async Task SaveBinaryAttachmentAsync(Stream bodyStream, string filename)
        {
            // Construct the full file path for the binary attachment
            string filePath = Path.Combine(_options.LogFileDirectory, filename);

            // Save the binary data to the specified file
            using (var fileStream = File.Create(filePath))
            {
                await bodyStream.CopyToAsync(fileStream);
            }
        }
    }
}
