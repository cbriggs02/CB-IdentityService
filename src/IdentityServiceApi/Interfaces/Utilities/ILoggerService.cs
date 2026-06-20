using IdentityServiceApi.Enums;

namespace IdentityServiceApi.Interfaces.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2026
    ///     @Updated: 2026
    /// </remarks>
    public interface ILoggerService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        void LogData(LogLevel logLevel, LogSource source, string message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        void LogData(LogLevel logLevel, LogSource source, string message, Exception ex);
    }
}
