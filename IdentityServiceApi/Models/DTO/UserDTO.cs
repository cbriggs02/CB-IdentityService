﻿using System.ComponentModel.DataAnnotations;

namespace IdentityServiceApi.Models.DTO
{
    /// <summary>
    ///     Data Transfer Object (DTO) representing a user with essential information.
    ///     Represents essential user data exposed through the User API to clients, 
    ///     ensuring only necessary information is shared.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class UserDTO
    {
        /// <summary>
        ///     Gets or sets the username of the user.
        /// </summary>
        [Required(ErrorMessage = "User name is required")]
        public string UserName { get; set; }

        /// <summary>
        ///     Gets or sets the first name of the user.
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        /// <summary>
        ///     Gets or sets the last name of the user.
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        /// <summary>
        ///     Gets or sets the email address of the user.
        /// </summary>
        [Required(ErrorMessage = "Email Address is required")]
        [EmailAddress]
        [StringLength(25)]
        public string Email { get; set; }

        /// <summary>
        ///     Gets or sets the phone number of the user.
        /// </summary>
        [Required(ErrorMessage = "Phone Number is required")]
        [Phone]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^\d{3}-\d{3}-\d{4}$", ErrorMessage = "Please enter a phone number in the format xxx-xxx-xxxx.")]
        public string PhoneNumber { get; set; }

        /// <summary>
        ///     Gets or sets the country of the user.
        /// </summary>
        [Required(ErrorMessage = "Country is required")]
        [StringLength(75)]
        public string Country { get; set; }
    }
}
