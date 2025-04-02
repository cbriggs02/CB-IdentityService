namespace IdentityServiceApi.Interfaces.Logging
{
    /// <summary>
    ///     Defines a contract for logging performance metrics related to the application's operations
    ///     using the audit logger service.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public interface IPerformanceLoggerService : IAuditLoggerService
    {
        /// <summary>
        ///     Asynchronously logs a performance issue when the response time exceeds a defined threshold.
        /// </summary>
        /// <param name="responseTime">
        ///     The time taken to process the request, in milliseconds.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the performance issue.
        /// </returns>
        Task LogSlowPerformanceAsync(long responseTime);
    }
}
