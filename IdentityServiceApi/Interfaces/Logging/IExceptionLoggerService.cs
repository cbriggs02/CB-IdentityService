namespace IdentityServiceApi.Interfaces.Logging
{
    /// <summary>
    ///     Defines a contract for logging exceptions that occur within the application
    ///     using the audit logger service.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public interface IExceptionLoggerService : IAuditLoggerService
    {
        /// <summary>
        ///     Asynchronously logs an exception that has been thrown in the application.
        /// </summary>
        /// <param name="exception">
        ///     The exception object that contains details about the error that occurred.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the exception.
        /// </returns>
        Task LogExceptionAsync(Exception exception);
    }
}
