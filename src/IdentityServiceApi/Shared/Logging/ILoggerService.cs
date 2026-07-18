namespace IdentityServiceApi.Shared.Logging
{
    /// <summary>
    ///     Defines a contract for logging application events in a consistent and structured manner.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2026
    ///     @Updated: 2026
    /// </remarks>
    public interface ILoggerService
    {
        /// <summary>
        ///     Logs an application event using the provided <see cref="LogEntry"/>.
        /// </summary>
        /// <param name="entry">
        ///     The log entry containing the log level, source, message, and optional exception details.
        /// </param>
        void LogData(LogEntry entry);
    }
}
