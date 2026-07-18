namespace IdentityServiceApi.Shared.Logging
{
    /// <summary>
    ///     Represents a structured log entry used for capturing application events.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2026
    ///     @Updated: 2026
    /// </remarks>
    public class LogEntry
    {
        /// <summary>
        ///     Gets or sets the severity level of the log entry.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        ///     Gets or sets the source of the log entry, indicating the originating component or feature.
        /// </summary
        public LogSource LogSource { get; set; }

        /// <summary>
        ///     Gets or sets the descriptive message associated with the log entry.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the exception associated with the log entry, if any.
        /// </summary>
        public Exception? Exception { get; set; }
    }
}
