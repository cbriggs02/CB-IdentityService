using IdentityServiceApi.Enums;
using IdentityServiceApi.Interfaces.Utilities;

namespace IdentityServiceApi.Services.Utilities
{
    /// <summary>
    /// 
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
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void LogData(LogLevel logLevel, LogSource source, string message)
        {
            _logger.Log(logLevel, "{Source} | {Message}", source, message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void LogData(LogLevel logLevel, LogSource source, string message, Exception ex)
        {
            _logger.Log(logLevel, ex, "{Source} | {Message}", source, message);
        }
    }
}
