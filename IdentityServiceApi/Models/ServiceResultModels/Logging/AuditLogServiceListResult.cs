using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.Shared;

namespace IdentityServiceApi.Models.ServiceResultModels.Logging
{
    /// <summary>
    ///     Represents the result of a service operation that retrieves a list of audit logs.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class AuditLogServiceListResult
    {
        /// <summary>
        ///     Gets or sets the collection of simplified audit logs returned from the service operation.
        /// </summary>
        public IEnumerable<SimplifiedAuditLogDTO> Logs { get; set; } = new List<SimplifiedAuditLogDTO>();

        /// <summary>
        ///     Gets or sets the pagination metadata associated with the audit log retrieval operation.
        ///     This metadata includes details such as total audit log count, current page, and page size. 
        /// </summary>
        public PaginationModel PaginationMetadata { get; set; } = new PaginationModel();
    }
}
