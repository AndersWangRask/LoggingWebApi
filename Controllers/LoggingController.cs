using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using LoggingWebApi.Services;
using LoggingWebApi.Configuration;

namespace LoggingWebApi.Controllers
{
    [ApiController]
    [Route("/")]
    public class LoggingController : ControllerBase
    {
        private readonly LoggingService _loggingService;
        private readonly LoggingOptions _options;

        public LoggingController(LoggingService loggingService, IOptions<LoggingOptions> options)
        {
            _loggingService = loggingService;
            _options = options.Value;
        }

        [HttpGet]
        [HttpPost]
        [HttpPut]
        [HttpDelete]
        [HttpPatch]
        [Route("{*url}")]
        public async Task<IActionResult> LogRequest()
        {
            var logEntry = await _loggingService.CreateLogEntryAsync(HttpContext);
            await _loggingService.SaveLogEntryAsync(logEntry);

            return StatusCode(_options.ResponseStatusCode);
        }
    }
}

