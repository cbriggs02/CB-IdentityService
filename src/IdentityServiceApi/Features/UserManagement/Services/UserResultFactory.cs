using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.DTOs;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.Results;

namespace IdentityServiceApi.Features.UserManagement.Services
{
    /// <summary>
    ///     Factory class for creating user service results, used for both successful and failed user operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class UserResultFactory : UserResultFactoryBase, IUserResultFactory
    {
        /// <summary>
        ///     Creates a failed user service result with specified errors.
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
        public override UserResult UserOperationFailure(string[] errors, ErrorType errorType) =>
            new() { Success = false, Errors = [.. errors], ErrorType = errorType };

        /// <summary>
        ///     Creates a successful user operation result with a user DTO.
        /// </summary>
        /// <param name="user">
        ///     The <see cref="UserDTO"/> representing the user information.
        /// </param>
        /// <returns>
        ///     A <see cref="UserResult"/> containing the success status and user data.
        /// </returns>
        public override UserResult UserOperationSuccess(UserDTO user) =>
            new() { Success = true, User = user };
    }
}
