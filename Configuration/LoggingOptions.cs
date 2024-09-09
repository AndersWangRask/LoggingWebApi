using System.ComponentModel.DataAnnotations;

namespace LoggingWebApi.Configuration
{
    public class LoggingOptions
    {
        [Required]
        public string LogFileDirectory { get; set; } = string.Empty;

        [Range(100, 599)]
        public int ResponseStatusCode { get; set; } = 200;
    }
}

