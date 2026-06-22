using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.DTOs;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.Results;
using IdentityServiceApi.Shared.Utilities;

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
    public abstract class UserResultFactoryBase(IParameterValidator parameterValidator) : ResultFactory(parameterValidator), IUserResultFactory
    {
        /// <summary>
        ///     Validates the properties of the given <see cref="UserDTO"/> to ensure all required fields are populated.
        /// </summary>
        /// <param name="user">
        ///     The <see cref="UserDTO"/> to validate.
        /// </param>
        protected void ValidateUserProperties(UserDTO user)
        {
            parameterValidator.ValidateNotNullOrEmpty(user.UserName, nameof(user.UserName));
            parameterValidator.ValidateNotNullOrEmpty(user.FirstName, nameof(user.FirstName));
            parameterValidator.ValidateNotNullOrEmpty(user.LastName, nameof(user.LastName));
            parameterValidator.ValidateNotNullOrEmpty(user.Email, nameof(user.Email));
            parameterValidator.ValidateNotNullOrEmpty(user.PhoneNumber, nameof(user.PhoneNumber));
        }

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
