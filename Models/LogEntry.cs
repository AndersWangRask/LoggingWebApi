using System;
using System.Collections.Generic;
using System.Linq;

namespace LoggingWebApi.Models
{
    /// <summary>
    /// Represents a log entry for an HTTP request, capturing various details about the request.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Gets the unique identifier for the log entry.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the timestamp when the log entry was created.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets or sets the HTTP method of the request (e.g., GET, POST).
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full URL of the request.
        /// </summary>
        public string RequestUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the path of the request.
        /// </summary>
        public string RequestPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the query string of the request.
        /// </summary>
        public string QueryString { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the IP address of the requestor.
        /// </summary>
        public string RequestorIp { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the headers of the request.
        /// </summary>
        public Dictionary<string, string> RequestHeaders { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the body of the request, if available.
        /// </summary>
        public string? RequestBody { get; set; }

        /// <summary>
        /// Gets a value indicating whether the request has a binary attachment.
        /// </summary>
        public bool HasBinaryAttachment => BinaryAttachment != null && BinaryAttachment.Length > 0;

        /// <summary>
        /// Gets or sets the filename for the binary attachment, if applicable.
        /// </summary>
        public string? BinaryAttachmentFilename { get; set; }

        /// <summary>
        /// Gets or sets the binary attachment associated with the request, if available.
        /// </summary>
        public MemoryStream? BinaryAttachment { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry"/> class.
        /// </summary>
        public LogEntry()
        {
            // Generate a unique identifier for this log entry
            Id = Guid.NewGuid().ToString();

            // Set the timestamp to the current UTC time
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Generates a filename for the log entry based on its timestamp and ID.
        /// </summary>
        /// <returns>A string representing the log filename.</returns>
        public string GetLogFilename()
        {
            //-->
            return $"{Timestamp:yyyyMMdd_HHmmss}_{Id}.txt";
        }

        /// <summary>
        /// Generates a filename for the binary attachment based on its timestamp and ID.
        /// </summary>
        /// <returns>A string representing the binary attachment filename.</returns>
        public string GetBinaryAttachmentFilename()
        {
            //-->
            return $"{Timestamp:yyyyMMdd_HHmmss}_{Id}.attachment";
        }

        /// <summary>
        /// Returns a string representation of the log entry, including all details and headers.
        /// </summary>
        /// <returns>A string that represents the current log entry.</returns>
        public override string ToString()
        {
            // Build the log entry string with basic details
            string logEntryString =
                $"Log Entry: {Id}\n" +
                $"Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss.fff}\n" +
                $"HTTP Method: {HttpMethod}\n" +
                $"Request URL: {RequestUrl}\n" +
                $"Request Path: {RequestPath}\n" +
                $"Query String: {QueryString}\n" +
                $"Requestor IP: {RequestorIp}\n" +
                $"Request Headers:\n{string.Join("\n", RequestHeaders.Select(h => $"  {h.Key}: {h.Value}"))}";

            // Append the request body if it exists
            if (!string.IsNullOrEmpty(RequestBody))
            {
                logEntryString += $"\n\nRequest Body:\n{RequestBody}";
            }

            // Append the binary attachment filename if a binary attachment is present
            if (HasBinaryAttachment)
            {
                logEntryString += $"\nBinary Attachment Filename: {BinaryAttachmentFilename}";
            }

            //--> Return the constructed log entry string
            return logEntryString;
        }
    }
}
