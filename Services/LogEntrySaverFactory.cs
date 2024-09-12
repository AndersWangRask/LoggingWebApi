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

        /// <summary>
        /// Creates a new instance of a log entry saver based on the configured saver type.
        /// </summary>
        /// <returns>
        /// The configured log entry saver type
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// When an invalid saver type is specified in the configuration.
        /// </exception>
        /// <remarks>
        /// It is possible to add as many saver types as needed.
        /// </remarks>
        public ILogEntrySaver CreateSaver()
        {
            // --> The configured log entry saver type
            return
                _options.SaverType.ToLower() switch
                {
                    "filesystem" => new FileSystemLogEntrySaver(_options),
                    "azureblob" => new AzureBlobLogEntrySaver(_options),
                    _ => throw new InvalidOperationException("Invalid saver type specified in configuration")
                };

            //It is possible to add as many saver types as needed
        }
    }
}
