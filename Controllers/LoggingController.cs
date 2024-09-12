using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using LoggingWebApi.Services;
using LoggingWebApi.Configuration;

namespace LoggingWebApi.Controllers
{
    /// <summary>
    /// API Controller for logging all HTTP requests received by the server.
    /// Supports multiple HTTP methods (GET, POST, PUT, DELETE, PATCH).
    /// </summary>
    [ApiController]
    [Route("/")]
    public class LoggingController : ControllerBase
    {
        /// <summary>
        /// The logging service used to create and save log entries.
        /// </summary>
        private readonly LoggingService _loggingService;

        /// <summary>
        /// Configuration options for logging, such as response status code.
        /// </summary>
        private readonly LoggingOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingController"/> class
        /// with the specified logging service and options.
        /// </summary>
        /// <param name="loggingService">The logging service used to log HTTP requests.</param>
        /// <param name="options">The logging options configuration.</param>
        public LoggingController(LoggingService loggingService, IOptions<LoggingOptions> options)
        {
            _loggingService = loggingService;
            _options = options.Value;
        }

        /// <summary>
        /// Handles all HTTP requests (GET, POST, PUT, DELETE, PATCH) and logs them.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> representing the result of the logging operation,
        /// with a status code specified in the logging options.</returns>
        [HttpGet]
        [HttpPost]
        [HttpPut]
        [HttpDelete]
        [HttpPatch]
        [Route("{*url}")]
        public async Task<IActionResult> LogRequest()
        {
            // Create a log entry for the incoming request
            Models.LogEntry logEntry = await _loggingService.CreateLogEntryAsync(HttpContext);

            // Save the log entry to the configured storage (e.g., file system)
            await _loggingService.LogEntrySaver.SaveLogEntry(logEntry);

            //--> Return the configured status code as the response
            return StatusCode(_options.ResponseStatusCode);
        }
    }
}