using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using LoggingWebApi.Models;
using LoggingWebApi.Configuration;

namespace LoggingWebApi.Services
{
    /// <summary>
    /// Provides logging services for HTTP requests, including creating log entries
    /// and saving them to disk.
    /// </summary>
    public class LoggingService
    {
        /// <summary>
        /// Configuration options for logging, provided through dependency injection.
        /// </summary>
        private readonly LoggingOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingService"/> class
        /// with the specified logging options.
        /// </summary>
        /// <param name="options">The logging options configuration.</param>
        public LoggingService(IOptions<LoggingOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// Reads the request body as a string and sets it in the log entry.
        /// </summary>
        /// <param name="context">The HttpContext of the Request</param>
        /// <param name="logEntry">The LogEntry to enrich with the body</param>
        /// <returns></returns>
        public async Task BodyToTextAsync(HttpContext context, LogEntry logEntry)
        {
            // Read the request body as a string
            using (StreamReader reader =
                new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: -1,
                    leaveOpen: true))
            {
                logEntry.RequestBody = await reader.ReadToEndAsync();

                // Reset the position of the request body stream to allow further processing
                context.Request.Body.Position = 0;
            }
        }

        /// <summary>
        /// Reads the request body as binary data and sets it in the log entry.
        /// </summary>
        /// <param name="context">The HttpContext of the Request</param>
        /// <param name="logEntry">The LogEntry to enrich with the body</param>
        /// <returns></returns>
        public async Task BodyToBinaryAsync(HttpContext context, LogEntry logEntry)
        {
            // Copy the request body into a MemoryStream
            MemoryStream memoryStream = new MemoryStream();
            await context.Request.Body.CopyToAsync(memoryStream);

            // Set the binary attachment and filename in the log entry
            memoryStream.Position = 0;
            logEntry.BinaryAttachment = memoryStream;

            // Reset the position of the request body stream to allow further processing
            context.Request.Body.Position = 0;
        }

        /// <summary>
        /// Creates a <see cref="LogEntry"/> based on the current HTTP context.
        /// </summary>
        /// <param name="context">The HTTP context containing the request data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the log entry.</returns>
        public async Task<LogEntry> CreateLogEntryAsync(HttpContext context)
        {
            // Initialize a new LogEntry with basic request details
            LogEntry logEntry =
                new LogEntry
                {
                    HttpMethod = context.Request.Method,
                    RequestUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}",
                    RequestPath = context.Request.Path,
                    QueryString = context.Request.QueryString.ToString(),
                    RequestorIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
                };

            // Add all request headers to the log entry
            foreach (var header in context.Request.Headers)
            {
                logEntry.RequestHeaders[header.Key] = header.Value.ToString();
            }

            // Check if there is a request body and if its length is greater than 0
            if (context.Request.ContentLength.HasValue && context.Request.ContentLength > 0)
            {
                // Enable buffering to allow reading the request body multiple times
                context.Request.EnableBuffering();

                // Validate if the content type is text, JSON, or XML
                if (context.Request.ContentType?.StartsWith("text/") == true ||
                    context.Request.ContentType == "application/json" ||
                    context.Request.ContentType == "application/xml")
                {
                    await BodyToTextAsync(context, logEntry);
                }
                else
                {
                    // Otherwise, treat the body as binary data
                    await BodyToBinaryAsync(context, logEntry);
                }
            }

            //--> Return the constructed log entry
            return logEntry;
        }

        /// <summary>
        /// Saves a <see cref="LogEntry"/> to a file and any associated binary attachments.
        /// </summary>
        /// <param name="logEntry">The log entry to save.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public async Task SaveLogEntryAsync(LogEntry logEntry)
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