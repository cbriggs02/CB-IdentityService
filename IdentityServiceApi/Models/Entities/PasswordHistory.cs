using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityServiceApi.Models.Entities
{
    /// <summary>
    ///     Represents an entity that stores historical user password information 
    ///     to ensure that users do not reuse previous passwords.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class PasswordHistory
    {
        /// <summary>
        ///     Gets or sets the unique identifier for the password history entry.
        /// </summary>
        [SwaggerSchema(ReadOnly = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? Id { get; set; }

        /// <summary>
        ///     Gets or sets the identifier of the user to whom this password belongs.
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the hashed password being stored in history.
        /// </summary>
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the date this password was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        ///     Gets or sets the navigation property to the user associated with this password history entry.
        /// </summary>
        public virtual User User { get; set; } = null!;
    }
}
