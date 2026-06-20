using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.Utilities;

namespace IdentityServiceApi.Features.UserManagement.Services
{
    /// <summary>
    ///     Factory class for creating user lookup service results, used for 
    ///     both successful and failed user lookup operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    /// <remarks>
    ///     Initializes a new instance of the <see cref="UserLookupResultFactory"/> class.
    /// </remarks>
    /// <param name="parameterValidator">
    ///     The parameter validator used to validate input parameters.
    /// </param>
    public class UserLookupResultFactory(IParameterValidator parameterValidator) : UserLookupResultFactoryBase(parameterValidator), IUserLookupResultFactory
    {
        /// <summary>
        ///     Creates a user lookup service result indicating a failure 
        ///     in the user lookup operation, along with any associated error messages.
        /// </summary>
        /// <param name="errors">An array of error messages that describe the reasons 
        ///     for the failure of the user lookup operation.</param>
        /// <returns>
        ///     A <see cref="UserLookupResult"/> object indicating failure 
        ///     and containing the list of error messages.
        /// </returns>
        public override UserLookupResult UserLookupOperationFailure(string[] errors)
        {
            ValidateErrors(errors);
            return new UserLookupResult { Success = false, Errors = [.. errors] };
        }

        /// <summary>
        ///     Creates a user lookup service result indicating a successful 
        ///     user lookup operation, including the details of the found user.
        /// </summary>
        /// <param name="user">
        ///     The user entity that was found during the lookup.
        /// </param>
        /// <returns>
        ///     A <see cref="UserLookupResult"/> object indicating success 
        ///     and containing the found user.
        /// </returns>
        public override UserLookupResult UserLookupOperationSuccess(User user)
        {
            _parameterValidator.ValidateObjectNotNull(user, nameof(user));
            return new UserLookupResult { Success = true, UserFound = user };
        }
    }
}
