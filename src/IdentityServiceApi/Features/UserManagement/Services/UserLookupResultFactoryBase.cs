using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.ResultFactories;
using IdentityServiceApi.Shared.Utilities;

namespace IdentityServiceApi.Features.UserManagement.Services
{
    /// <summary>
    ///     Base class for creating user lookup service results, used for 
    ///     both successful and failed user lookup operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public abstract class UserLookupResultFactoryBase(IParameterValidator parameterValidator) : ResultFactory(parameterValidator), IUserLookupResultFactory
    {
        /// <summary>
        ///     Creates a failed user lookup service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="UserLookupResult"/> indicating failure along with the provided errors.
        /// </returns>
        public abstract UserLookupResult UserLookupOperationFailure(string[] errors);

        /// <summary>
        ///     Creates a successful user lookup service result with the specified user.
        /// </summary>
        /// <param name="user">
        ///     The <see cref="User"/> object representing the successfully looked up user.
        /// </param>
        /// <returns>
        ///     A <see cref="UserLookupResult"/> containing the success status and the user data.
        /// </returns>
        public abstract UserLookupResult UserLookupOperationSuccess(User user);
    }
}
