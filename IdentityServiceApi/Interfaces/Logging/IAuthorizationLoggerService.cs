namespace IdentityServiceApi.Interfaces.Logging
{
    /// <summary>
    ///     Defines a contract for logging authorization breaches within the application
    ///     using the audit logger service.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public interface IAuthorizationLoggerService : IAuditLoggerService
    {
        /// <summary>
        ///     Asynchronously logs an authorization breach by a specific user.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the authorization breach.
        /// </returns>
        Task LogAuthorizationBreachAsync();
    }
}
