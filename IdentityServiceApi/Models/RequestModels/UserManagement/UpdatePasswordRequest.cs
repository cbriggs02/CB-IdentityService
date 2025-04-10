using System.ComponentModel.DataAnnotations;

namespace IdentityServiceApi.Models.RequestModels.UserManagement
{
    /// <summary>
    ///     Represents the model used when a user is updating their password.
    ///     This model contains the current and new password required for a password change request.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class UpdatePasswordRequest
    {
        /// <summary>
        ///     Gets or sets the current password of the user, which is required to validate the user's identity.
        /// </summary>
        [Required(ErrorMessage = "Current Password is required")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the new password that the user wants to set.
        ///     This will replace the current password once validation is successful.
        /// </summary>
        [Required(ErrorMessage = "New Password is required")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
