using IdentityServiceApi.Models.RequestModels.Authentication;
using IdentityServiceApi.Models.ServiceResultModels.Authentication;

namespace IdentityServiceApi.Interfaces.Authentication
{
    /// <summary>
    ///     Interface defining the contract for a service responsible for login-related operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public interface ILoginService
    {
        /// <summary>
        ///     Asynchronously logs in a user to the system using provided credentials.
        /// </summary>
        /// <param name="credentials">
        ///     A model object that contains the necessary information for authentication, including the
        ///     user's username and password.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns an <see cref="LoginServiceResult"/> object.
        ///     The result indicates the success or failure of the login attempt, along with any relevant messages
        ///     or tokens.
        /// </returns>
        Task<LoginServiceResult> LoginAsync(LoginRequest credentials);
    }
}
