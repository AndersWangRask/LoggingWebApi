using System;
using System.Collections.Generic;
using System.Linq;

namespace LoggingWebApi.Models
{
    public class LogEntry
    {
        public string Id { get; }
        public DateTime Timestamp { get; }
        public string HttpMethod { get; set; } = string.Empty;
        public string RequestUrl { get; set; } = string.Empty;
        public string RequestPath { get; set; } = string.Empty;
        public string QueryString { get; set; } = string.Empty;
        public string RequestorIp { get; set; } = string.Empty;
        public Dictionary<string, string> RequestHeaders { get; set; } = new Dictionary<string, string>();
        public string? RequestBody { get; set; }
        public bool HasBinaryAttachment { get; set; }
        public string? BinaryAttachmentFilename { get; set; }

        public LogEntry()
        {
            Id = Guid.NewGuid().ToString();
            Timestamp = DateTime.UtcNow;
        }

        public string GetLogFilename()
        {
            return $"{Timestamp:yyyyMMdd_HHmmss}_{Id}.txt";
        }

        public string GetBinaryAttachmentFilename()
        {
            return $"{Timestamp:yyyyMMdd_HHmmss}_{Id}.attachment";
        }

        public override string ToString()
        {
            var logEntryString = $"Log Entry: {Id}\n" +
                   $"Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss.fff}\n" +
                   $"HTTP Method: {HttpMethod}\n" +
                   $"Request URL: {RequestUrl}\n" +
                   $"Request Path: {RequestPath}\n" +
                   $"Query String: {QueryString}\n" +
                   $"Requestor IP: {RequestorIp}\n" +
                   $"Request Headers:\n{string.Join("\n", RequestHeaders.Select(h => $"  {h.Key}: {h.Value}"))}";

            if (!string.IsNullOrEmpty(RequestBody))
            {
                logEntryString += $"\n\nRequest Body:\n{RequestBody}";
            }

            if (HasBinaryAttachment)
            {
                logEntryString += $"\nBinary Attachment Filename: {BinaryAttachmentFilename}";
            }

            return logEntryString;
        }
    }
}

