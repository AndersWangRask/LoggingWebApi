using LoggingWebApi.Models;

namespace LoggingWebApi.Interfaces
{
    public interface ILogEntrySaver
    {
        /// <summary>
        /// Saves the log entry to a persistent store.
        /// </summary>
        /// <param name="logEntry">The log entry to save.</param>
        Task SaveLogEntry(LogEntry logEntry);
    }
}