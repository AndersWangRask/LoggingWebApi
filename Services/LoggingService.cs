using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using LoggingWebApi.Models;
using LoggingWebApi.Configuration;
using System.Text.RegularExpressions;
using LoggingWebApi.Interfaces;

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
        /// A dictionary of body readers for different content types.
        /// </summary>
        private Dictionary<string, Func<HttpContext, LogEntry, Task>> _bodyReaders =
            new Dictionary<string, Func<HttpContext, LogEntry, Task>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingService"/> class
        /// with the specified logging options.
        /// </summary>
        /// <param name="options">The logging options configuration.</param>
        public LoggingService(IOptions<LoggingOptions> options)
        {
            _options = options.Value;

            // Initialize the body readers for different content types
            _bodyReaders.Add(@"^application/json$", BodyToTextAsync);
            _bodyReaders.Add(@"^application/xml$", BodyToTextAsync);
            _bodyReaders.Add(@"^text/.*", BodyToTextAsync);
            _bodyReaders.Add(@"^.*$", BodyToBinaryAsync);
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
        /// Gets the appropriate body reader based on the content type.
        /// </summary>
        /// <param name="contentType">
        /// The content type of the request.
        /// Based on this value, the appropriate body reader will be returned.
        /// </param>
        /// <returns>
        /// The body reader function that can be used to read the request body.
        /// </returns>
        public Func<HttpContext, LogEntry, Task> GetBodyReader(string? contentType)
        {
            // Find the appropriate body reader based on the content type
            // If no reader is found, default to reading the body as binary data
            if (string.IsNullOrWhiteSpace(contentType))
            {
                //--> Return the default body reader for binary data
                return BodyToBinaryAsync;
            }

            Func<HttpContext, LogEntry, Task> bodyReader =
                _bodyReaders
                    .FirstOrDefault(kvp => Regex.IsMatch(contentType, kvp.Key, RegexOptions.IgnoreCase))
                    .Value ?? BodyToBinaryAsync;

            //--> Return the appropriate body reader based on the content type
            return bodyReader;
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

                // Get the content type of the request
                string? contentType = context.Request.ContentType;

                // Get the appropriate body reader based on the content type
                Func<HttpContext, LogEntry, Task> bodyReader = GetBodyReader(contentType);

                // Read the request body based on the content type
                await bodyReader(context, logEntry);
            }

            //--> Return the constructed log entry
            return logEntry;
        }

        /// <summary>
        /// Gets the <see cref="ILogEntrySaver"/> instance for saving log entries.
        /// </summary>
        public ILogEntrySaver LogEntrySaver 
        { 
            get
            {
                if (_logEntrySaver == null)
                {
                    _logEntrySaver = new LogEntrySaver(_options);
                }

                //--> Return the log entry saver
                return _logEntrySaver;
            }
        }
        private ILogEntrySaver _logEntrySaver;
    }
}