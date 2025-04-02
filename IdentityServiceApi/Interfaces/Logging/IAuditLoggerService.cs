using IdentityServiceApi.Models.RequestModels.Logging;
using IdentityServiceApi.Models.ServiceResultModels.Logging;
using IdentityServiceApi.Models.ServiceResultModels.Shared;

namespace IdentityServiceApi.Interfaces.Logging
{
    /// <summary>
    ///     Interface that defines methods for logging audit entries and managing audit logs.
    ///     It includes operations for retrieving, deleting, and managing audit log records.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public interface IAuditLoggerService
    {
        /// <summary>
        ///     Asynchronously retrieves a paginated list of audit logs based on the provided request parameters.
        /// </summary>
        /// <param name="request">
        ///     The request parameters including pagination details such as page number, size, and filters.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation of retrieving logs. 
        ///     The result contains a list of audit logs along with pagination metadata.
        /// </returns>
        Task<AuditLogServiceListResult> GetLogsAsync(AuditLogListRequest request);

        /// <summary>
        ///     Asynchronously deletes a audit log in the system by ID.
        /// </summary>
        /// <param name="id">
        ///     The ID used to locate the audit log to be deleted in the system.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns a <see cref= ServiceResult"/> object.
        /// </returns>
        Task<ServiceResult> DeleteLogAsync(string id);
    }
}
