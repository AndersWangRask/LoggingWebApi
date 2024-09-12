using Microsoft.Extensions.Options;
using LoggingWebApi.Configuration;
using LoggingWebApi.Interfaces;

namespace LoggingWebApi.Services
{
    public class LogEntrySaverFactory : ILogEntrySaverFactory
    {
        private readonly LoggingOptions _options;

        public LogEntrySaverFactory(IOptions<LoggingOptions> options)
        {
            _options = options.Value;
        }

        public LogEntrySaverFactory(LoggingOptions options)
        {
            _options = options;
        }

        public ILogEntrySaver CreateSaver()
        {
            // Placeholder Logic
            return new FileSystemLogEntrySaver(_options);

            //TODO: Implement logic to choose the appropriate saver based on configuration, AWR, 2024-09-12

            // Example logic to choose the appropriate saver based on configuration
            //return _options.SaverType switch
            //{
            //    "FileSystem" => new FileSystemLogEntrySaver(_options),
            //    "AzureBlob" => new AzureBlobLogEntrySaver(_options),
            //    "Database" => new DatabaseLogEntrySaver(_options),
            //    _ => throw new InvalidOperationException("Invalid saver type specified in configuration")
            //};
        }
    }
}
