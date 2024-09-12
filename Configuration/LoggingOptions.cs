using System.ComponentModel.DataAnnotations;

namespace LoggingWebApi.Configuration
{
    public class LoggingOptions
    {
        public string LogFileDirectory { get; set; } = string.Empty;

        public string SaverType { get; set; } = string.Empty;

        public string AzureBlobConnectionString { get; set; } = string.Empty;
        
        public string AzureBlobContainerName { get; set; } = string.Empty;

        [Range(100, 599)]
        public int ResponseStatusCode { get; set; } = 200;
    }
}