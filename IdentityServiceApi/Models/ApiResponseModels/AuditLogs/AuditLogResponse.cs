using IdentityServiceApi.Models.DTO;

namespace IdentityServiceApi.Models.ApiResponseModels.AuditLogs
{
    /// <summary>
    ///     Represents the API response model that encapsulates 
    ///     audit log information returned from audit-related operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    /// </remarks>
    public class AuditLogResponse
    {
        /// <summary>
        ///     Gets or sets the audit log data associated with the operation.
        /// </summary>
        public AuditLogDTO AuditLog { get; set; } = new AuditLogDTO();
    }
}
