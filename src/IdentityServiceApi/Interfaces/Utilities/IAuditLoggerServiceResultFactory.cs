using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.ServiceResultModels.Logging;

namespace IdentityServiceApi.Interfaces.Utilities
{
    /// <summary>
    ///     Defines a factory interface for creating standardized service results 
    ///     specific to audit logging operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    /// </remarks>
    public interface IAuditLoggerServiceResultFactory : IServiceResultFactory
    {
        /// <summary>
        ///     Creates an <see cref="AuditLogServiceResult"/> that represents a failed audit logging operation.
        /// </summary>
        /// <param name="errors">
        ///     A collection of error messages describing the reasons for the failure.
        /// </param>
        /// <returns>
        ///     An <see cref="AuditLogServiceResult"/> containing the error details.
        /// </returns>
        AuditLogServiceResult AuditLoggerOperationFailure(string[] errors);

        /// <summary>
        ///     Creates an <see cref="AuditLogServiceResult"/> that represents a successful audit logging operation.
        /// </summary>
        /// <param name="auditLog">
        ///     The <see cref="AuditLogDTO"/> representing the successfully recorded audit entry.
        /// </param>
        /// <returns>
        ///     An <see cref="AuditLogServiceResult"/> containing the audit log data.
        /// </returns>
        AuditLogServiceResult AuditLoggerOperationSuccess(AuditLogDTO auditLog);
    }
}
