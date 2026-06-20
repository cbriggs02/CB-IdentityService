using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.ServiceResultModels.UserManagement;
using IdentityServiceApi.Services.Utilities.ResultFactories.BaseClasses;

namespace IdentityServiceApi.Services.Utilities.ResultFactories.UserManagement
{
    /// <summary>
    ///     Factory class for creating user service results, used for both successful and failed user operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class UserServiceResultFactory(IParameterValidator parameterValidator) : UserServiceResultFactoryBase(parameterValidator), IUserServiceResultFactory
    {

        /// <summary>
        ///     Creates a failed user service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="UserServiceResult"/> indicating failure along with the provided errors.
        /// </returns>
        public override UserServiceResult UserOperationFailure(string[] errors)
        {
            ValidateErrors(errors);
            return new UserServiceResult { Success = false, Errors = [.. errors] };
        }

        /// <summary>
        ///     Creates a successful user operation result with a user DTO.
        /// </summary>
        /// <param name="user">
        ///     The <see cref="UserDTO"/> representing the user information.
        /// </param>
        /// <returns>
        ///     A <see cref="UserServiceResult"/> containing the success status and user data.
        /// </returns>
        public override UserServiceResult UserOperationSuccess(UserDTO user)
        {
            _parameterValidator.ValidateObjectNotNull(user, nameof(user));
            ValidateUserProperties(user);
            return new UserServiceResult { Success = true, User = user };
        }
    }
}
