using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.ServiceResultModels.UserManagement;
using IdentityServiceApi.Services.Utilities.ResultFactories.Common;

namespace IdentityServiceApi.Services.Utilities.ResultFactories.AbstractClasses
{
    /// <summary>
    ///     Base class for creating user service results, used for both successful and failed user operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public abstract class UserServiceResultFactoryBase : ServiceResultFactory, IUserServiceResultFactory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UserServiceResultFactoryBase"/> class.
        /// </summary>
        /// <param name="parameterValidator">
        ///     The parameter validator used to validate input parameters.
        /// </param>
        protected UserServiceResultFactoryBase(IParameterValidator parameterValidator) : base(parameterValidator)
        {
        }

        /// <summary>
        ///     Validates the properties of the given <see cref="UserDTO"/> to ensure all required fields are populated.
        /// </summary>
        /// <param name="user">
        ///     The <see cref="UserDTO"/> to validate.
        /// </param>
        protected void ValidateUserProperties(UserDTO user)
        {
            _parameterValidator.ValidateNotNullOrEmpty(user.UserName, nameof(user.UserName));
            _parameterValidator.ValidateNotNullOrEmpty(user.FirstName, nameof(user.FirstName));
            _parameterValidator.ValidateNotNullOrEmpty(user.LastName, nameof(user.LastName));
            _parameterValidator.ValidateNotNullOrEmpty(user.Email, nameof(user.Email));
            _parameterValidator.ValidateNotNullOrEmpty(user.PhoneNumber, nameof(user.PhoneNumber));
        }

        /// <summary>
        ///     Creates a failed user operation service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="UserServiceResult"/> indicating failure along with the provided errors.
        /// </returns>
        public abstract UserServiceResult UserOperationFailure(string[] errors);

        /// <summary>
        ///     Creates a successful user operation service result with the specified user data.
        /// </summary>
        /// <param name="user">
        ///     The <see cref="UserDTO"/> representing the successfully created or updated user.
        /// </param>
        /// <returns>
        ///     A <see cref="UserServiceResult"/> containing the success status and the user data.
        /// </returns>
        public abstract UserServiceResult UserOperationSuccess(UserDTO user);
    }
}
