using IdentityServiceApi.Features.UserManagement.Models.DTOs;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.Results;

namespace IdentityServiceApi.Features.UserManagement.Interfaces
{
    /// <summary>
    ///     Interface for creating service results related to user operations.
    ///     This interface defines methods for creating both success and failure results
    ///     for user-related operations, such as creating or updating users, etc...
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public interface IUserResultFactory : IResultFactory
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
        UserResult UserOperationFailure(string[] errors, ErrorType errorType);

        /// <summary>
        ///     Creates a successful user operation service result with the specified user data.
        /// </summary>
        /// <param name="user">
        ///     The <see cref="UserDTO"/> representing the successfully created or updated user.
        /// </param>
        /// <returns>
        ///     A <see cref="UserResult"/> containing the success status and the user data.
        /// </returns>
        UserResult UserOperationSuccess(UserDTO user);
    }
}
