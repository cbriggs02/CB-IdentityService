using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.DTOs;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.Results;

namespace IdentityServiceApi.Features.UserManagement.Services
{
    /// <summary>
    ///     Base class for creating user service results, used for both successful and failed user operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public abstract class UserResultFactoryBase : ResultFactory, IUserResultFactory
    {
        /// <summary>
        ///     Creates a failed user operation service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <param name="errorType">
        ///     An <see cref="ErrorType"/> indicating the type of error that occurred during the user operation.
        /// </param>
        /// <returns>
        ///     A <see cref="UserResult"/> indicating failure along with the provided errors.
        /// </returns>
        public abstract UserResult UserOperationFailure(string[] errors, ErrorType errorType);

        /// <summary>
        ///     Creates a successful user operation service result with the specified user data.
        /// </summary>
        /// <param name="user">
        ///     The <see cref="UserDTO"/> representing the successfully created or updated user.
        /// </param>
        /// <returns>
        ///     A <see cref="UserResult"/> containing the success status and the user data.
        /// </returns>
        public abstract UserResult UserOperationSuccess(UserDTO user);
    }
}
