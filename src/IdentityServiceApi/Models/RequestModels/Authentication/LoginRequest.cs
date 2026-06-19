using System.ComponentModel.DataAnnotations;

namespace IdentityServiceApi.Models.RequestModels.Authentication
{
    /// <summary>
    ///     Represents the model that encapsulates data used for user authentication.
    ///     This model contains the credentials of a user requesting a token from the login API and service,
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class LoginRequest
    {
        /// <summary>
        ///     Gets or sets the username for authentication.
        ///     Used to identify the user during the authentication process.
        /// </summary>
        [Required(ErrorMessage = "Username is required.")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the password for authentication.
        ///     Used to verify the user's identity during the authentication process.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
