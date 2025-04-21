
namespace IdentityServiceApi.Models.ApiResponseModels.Login
{
    /// <summary>
    ///     Represents the response returned by the login API after a successful user login.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class LoginResponse
    {
        /// <summary>
        ///    The JWT (JSON Web Token) that is generated upon a successful login from login service.
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }
}
