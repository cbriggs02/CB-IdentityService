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
        /// <summary>
        ///     Logs an application event using the provided <see cref="LogEntry"/>.
        /// </summary>
        /// <param name="entry">
        ///     The log entry containing the log level, source, message, and optional exception information.
        /// </param>
        public void LogData(LogEntry entry)
        {
            var sanitizedSource = SanitizeForLog(entry.LogSource.ToString());
            var sanitizedMessage = SanitizeForLog(entry.Message);

            if (entry.Exception != null)
            {
                logger.Log(entry.LogLevel, entry.Exception, "{Source} | {Message}", sanitizedSource, sanitizedMessage);
            } 
            else
            {
                logger.Log(entry.LogLevel, "{Source} | {Message}", sanitizedSource, sanitizedMessage);
            }
        }

        private static string SanitizeForLog(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace("\r", " ").Replace("\n", " ");
        }
    }
}
