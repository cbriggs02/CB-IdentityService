using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace IdentityServiceApi.Models.Entities
{
    /// <summary>
    ///     Represents the user entity, extending the IdentityUser class from ASP.NET Core Identity.
    ///     This entity adds custom properties to model user data specific to this application.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class User : IdentityUser
    {
        /// <summary>
        ///     Gets or sets the first name of the user.
        /// </summary>
        [Required]
        [PersonalData]
        [StringLength(50)]
        public string FirstName { get; set; }

        /// <summary>
        ///     Gets or sets the last name of the user.
        /// </summary>
        [Required]
        [PersonalData]
        [StringLength(50)]
        public string LastName { get; set; }

        /// <summary>
        ///     Gets or sets the country identifier for the user.
        /// </summary>
        public int CountryId { get; set; }

        /// <summary>
        ///     Gets or sets the account status of the user.
        ///     Indicates whether the user account is (1) active or (0) inactive,
        /// </summary>
        public int AccountStatus { get; set; }

        /// <summary>
        ///     Gets or sets the datetime when the user account was created.
        ///     Used for tracking the account creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     Gets or sets the datetime when the user account was last updated.
        ///     This indicates when the user information was last modified.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        ///     Navigation property for the user's country.
        /// </summary>
        public virtual Country Country { get; set; }

        /// <summary>
        ///     Gets or sets a collection of previous passwords used by this user.
        ///     This collection helps track password history to prevent password reuse.
        /// </summary>
        public virtual ICollection<PasswordHistory> Passwords { get; set; } = new List<PasswordHistory>();

        /// <summary>
        ///     Gets or sets a collection of audit logs associated with this user.
        ///     This collection helps track actions, exceptions, or authorization breaches triggered by this user.
        /// </summary>
        public virtual ICollection<AuditLog> Logs { get; set; } = new List<AuditLog>();
    }
}
