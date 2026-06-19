using IdentityServiceApi.Models.ServiceResultModels.Authentication;

namespace IdentityServiceApi.Interfaces.Utilities
{
    /// <summary>
    ///     Interface for creating service results related to login operations.
    ///     This interface defines methods for creating both success and failure results
    ///     for login operations, including handling the authentication token for successful logins.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public interface ILoginServiceResultFactory : IServiceResultFactory
    {
        /// <summary>
        ///     Creates a failed login service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="LoginServiceResult"/> indicating failure along with the provided errors.
        /// </returns>
        LoginServiceResult LoginOperationFailure(string[] errors);

        /// <summary>
        ///     Creates a successful login service result with a token.
        /// </summary>
        /// <param name="token">
        ///     The authentication token generated upon successful login.
        /// </param>
        /// <returns>
        ///     A <see cref="LoginServiceResult"/> containing the success status and the token.
        /// </returns>
        LoginServiceResult LoginOperationSuccess(string token);
    }
}
