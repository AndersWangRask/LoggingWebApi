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
    public class LoggingService
    {
        private readonly LoggingOptions _options;

        public LoggingService(IOptions<LoggingOptions> options)
        {
            _options = options.Value;
        }

        public async Task<LogEntry> CreateLogEntryAsync(HttpContext context)
        {
            var logEntry =
                new LogEntry
                {
                    HttpMethod = context.Request.Method,
                    RequestUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}",
                    RequestPath = context.Request.Path,
                    QueryString = context.Request.QueryString.ToString(),
                    RequestorIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
                };

            foreach (var header in context.Request.Headers)
            {
                logEntry.RequestHeaders[header.Key] = header.Value.ToString();
            }

            if (context.Request.ContentLength > 0)
            {
                context.Request.EnableBuffering();
                using (var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: -1,
                    leaveOpen: true))
                {
                    var body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;

                    if (context.Request.ContentType?.StartsWith("text/") == true ||
                        context.Request.ContentType == "application/json" ||
                        context.Request.ContentType == "application/xml")
                    {
                        logEntry.RequestBody = body;
                    }
                    else
                    {
                        logEntry.HasBinaryAttachment = true;
                        logEntry.BinaryAttachmentFilename = logEntry.GetBinaryAttachmentFilename();
                        await SaveBinaryAttachmentAsync(context.Request.Body, logEntry.BinaryAttachmentFilename);
                    }
                }
            }

            return logEntry;
        }

        public async Task SaveLogEntryAsync(LogEntry logEntry)
        {
            var logFilePath = Path.Combine(_options.LogFileDirectory, logEntry.GetLogFilename());
            await File.WriteAllTextAsync(logFilePath, logEntry.ToString());
        }

        private async Task SaveBinaryAttachmentAsync(Stream bodyStream, string filename)
        {
            var filePath = Path.Combine(_options.LogFileDirectory, filename);
            using (var fileStream = File.Create(filePath))
            {
                await bodyStream.CopyToAsync(fileStream);
            }
        }
    }
}