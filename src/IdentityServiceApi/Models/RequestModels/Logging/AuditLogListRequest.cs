using IdentityServiceApi.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace IdentityServiceApi.Models.RequestModels.Logging
{
    /// <summary>
    ///     Represents the model used when requesting a paginated list of audit logs.
    ///     It contains parameters to specify the page number, page size and optional audit action for filtering.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class AuditLogListRequest
    {
        /// <summary>
        ///     Gets or sets the page number of the audit log list being requested.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0.")]
        public int Page { get; set; }

        /// <summary>
        ///     Gets or sets the size of each page of the audit log list being requested.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page size must be greater than 0.")]
        public int PageSize { get; set; }

        /// <summary>
        ///     Gets or sets the audit action for filtering actions logged.
        ///     0 for authorization breach, 1 for exceptions, 2 for slow performance.
        /// </summary>
        public AuditAction? Action { get; set; }
    }
}
