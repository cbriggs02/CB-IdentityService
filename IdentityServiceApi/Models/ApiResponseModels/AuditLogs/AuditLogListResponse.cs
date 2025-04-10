using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.Shared;

namespace IdentityServiceApi.Models.ApiResponseModels.AuditLogsResponses
{
    /// <summary>
    ///     Represents the response returned by the audit log API when retrieving audit logs with pagination.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class AuditLogListResponse
    {
        /// <summary>
        ///     The list of audit logs retrieved from the service that is returned by the API.
        ///     This collection may be empty if no audit logs are available.
        /// </summary>
        public IEnumerable<AuditLogDTO> Logs { get; set; } = new List<AuditLogDTO>();

        /// <summary>
        ///     Metadata for pagination, such as total count and page details.
        /// </summary>
        public PaginationModel PaginationMetadata { get; set; } = new PaginationModel();
    }
}
