using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.ServiceResultModels.Shared;

namespace IdentityServiceApi.Models.ServiceResultModels.Logging
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public class AuditLogServiceResult : ServiceResult
    {
        /// <summary>
        /// 
        /// </summary>
        public AuditLogDTO AuditLog { get; set; } = new AuditLogDTO();
    }
}
