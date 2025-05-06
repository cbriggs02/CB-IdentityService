using IdentityServiceApi.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityServiceApi.Models.DTO
{
    /// <summary>
    ///     Represents a simplified data transfer object (DTO) for audit log entries,
    ///     containing essential details such as the unique identifier, action performed, and timestamp.
    /// </summary>
	/// <remarks>
	///     @Author: Christian Briglio
	///     @Created: 2025
	/// </remarks>
	public class SimplifiedAuditLogDTO
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
		///     Gets or sets the timestamp indicating when the action occurred.
		///     This property captures the exact time of the log entry in UTC.
		/// </summary>
		public DateTime TimeStamp { get; set; }
	}
}
