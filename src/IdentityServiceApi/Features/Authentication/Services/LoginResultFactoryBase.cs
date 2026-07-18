using IdentityServiceApi.Features.Authentication.Interfaces;
using IdentityServiceApi.Features.Authentication.Models;
using IdentityServiceApi.Shared.Results;

namespace IdentityServiceApi.Features.Authentication.Services
{
    /// <summary>
    ///     Base class for creating login service results, used for both successful and failed login operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public abstract class LoginResultFactoryBase : ResultFactory, ILoginResultFactory
    {
        /// <summary>
        ///     Creates a failed login service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <param name="errorType">
        ///     An <see cref="ErrorType"/> indicating the type of error that occurred during the login operation.
        /// </param>
        /// <returns>
        ///     A <see cref="LoginResult"/> indicating failure along with the provided errors.
        /// </returns>
        public abstract LoginResult LoginOperationFailure(string[] errors, ErrorType errorType);

        /// <summary>
        ///     Creates a successful login service result with a token.
        /// </summary>
        /// <param name="token">
        ///     The authentication token generated upon successful login.
        /// </param>
        /// <returns>
        ///     A <see cref="LoginResult"/> containing the success status and the token.
        /// </returns>
        public abstract LoginResult LoginOperationSuccess(string token);
    }
}
