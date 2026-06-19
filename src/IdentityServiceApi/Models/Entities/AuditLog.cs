using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityServiceApi.Models.Entities
{
    /// <summary>
    ///     Represents the type of action logged in the audit log.
    ///     This enum captures specific operations, such as security breaches or exceptions, 
    ///     that are recorded for auditing purposes.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public enum AuditAction
    {
        AuthorizationBreach,
        Exception,
        SlowPerformance
    }

    /// <summary>
    ///     Represents an audit log entry, capturing actions performed within the application.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class AuditLog
    {
        /// <summary>
        ///     Gets or sets the unique identifier for the audit log entry.
        /// </summary>
        [SwaggerSchema(ReadOnly = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? Id { get; set; }

        /// <summary>
        ///     Gets or sets the action performed that is being logged.
        ///     The action is described by the <see cref="AuditAction"/> enum, which specifies
        ///     operations like unauthorized access attempts or exceptions.
        /// </summary>
        [Column(TypeName = "varchar(50)")]
        public AuditAction Action { get; set; }

        /// <summary>
        ///     Gets or sets the identifier of the user who performed the action.
        ///     This value represents the unique ID of the user in the system.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        ///     Gets or sets detailed information about the action or exception being logged.
        ///     This may include error messages or additional context regarding the action.
        /// </summary>
        [Required]
        [Column(TypeName = "NVARCHAR(MAX)")]
        public string Details { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the IP address from which the action was performed.
        ///     This information can be useful for tracking the origin of requests.
        /// </summary>
        [Required]
        [StringLength(40)]
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the timestamp indicating when the action occurred.
        ///     This property captures the exact time of the log entry in UTC.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        ///     Navigation property to the associated user.
        ///     This property allows accessing user details related to the audit log entry.
        /// </summary>
        public virtual User? User { get; set; }
    }
}
