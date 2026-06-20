namespace IdentityServiceApi.Shared.Logging
{
    /// <summary>
    ///     Provides an implementation of <see cref="ILoggerService"/> for logging application events.
    ///     Wraps the Microsoft <see cref="ILogger"/> to standardize structured logging across the application.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2026
    ///     @Updated: 2026
    /// </remarks>
    public class LoggerService(ILogger<LoggerService> logger) : ILoggerService
    {
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        ///     Logs an application event using the provided <see cref="LogEntry"/>.
        /// </summary>
        /// <param name="entry">
        ///     The log entry containing the log level, source, message, and optional exception information.
        /// </param>
        public void LogData(LogEntry entry)
        {
            if (entry.Exception != null)
            {
                _logger.Log(entry.LogLevel, entry.Exception, "{Source} | {Message}", entry.LogSource, entry.Message);
            }

            _logger.Log(entry.LogLevel, "{Source} | {Message}", entry.LogSource, entry.Message);
        }
    }
}
