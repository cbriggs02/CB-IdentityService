using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace IdentityServiceApi.Features.UserManagement.Models.DTOs
{
    /// <summary>
    ///     Data Transfer Object (DTO) representing a user with essential information.
    ///     Represents essential user data exposed through the User API to clients, 
    ///     ensuring only necessary information is shared.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class UserDTO
    {
        /// <summary>
        ///     Gets or sets the unique identifier for the user (from ASP.NET Identity).
        /// </summary>
        [SwaggerSchema(ReadOnly = true)]
        public string? Id { get; set; }

        /// <summary>
        ///     Gets or sets the username of the user.
        /// </summary>
        [Required(ErrorMessage = "User name is required")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the first name of the user.
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the last name of the user.
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the email address of the user.
        /// </summary>
        [Required(ErrorMessage = "Email Address is required")]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the phone number of the user.
        /// </summary>
        [Required(ErrorMessage = "Phone Number is required")]
        [Phone]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^\d{3}-\d{3}-\d{4}$", ErrorMessage = "Please enter a phone number in the format xxx-xxx-xxxx.")]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the country identifier for the user.
        /// </summary>
        public int CountryId { get; set; }

        /// <summary>
        ///     Gets or sets the name of the country for the user.
        /// </summary>
        public string? CountryName { get; set; }

        /// <summary>
        ///     Gets or sets the account status of the user.
        /// </summary>
        public int AccountStatus { get; set; }

        /// <summary>
        ///     Gets or sets the role identifier associated with the user.
        /// </summary>
        public string? RoleId { get; set; }
    }
}

