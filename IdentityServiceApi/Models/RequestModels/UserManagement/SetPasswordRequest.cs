using System.ComponentModel.DataAnnotations;

namespace IdentityServiceApi.Models.RequestModels.UserManagement
{
    /// <summary>
    ///     Represents the model used by a user to set a new password.
    ///     Typically used during account creation or password reset.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class SetPasswordRequest
    {
        /// <summary>
        ///     Gets or sets the new password the user is setting for their account.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the confirmed password that should match the new password.
        ///     This is required to ensure the user correctly enters the password twice.
        /// </summary>
        [Required(ErrorMessage = "Confirmed Password is required")]
        [DataType(DataType.Password)]
        public string PasswordConfirmed { get; set; } = string.Empty;
    }
}
