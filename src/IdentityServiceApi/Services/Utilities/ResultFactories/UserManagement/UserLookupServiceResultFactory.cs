using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.ServiceResultModels.UserManagement;
using IdentityServiceApi.Services.Utilities.ResultFactories.BaseClasses;

namespace IdentityServiceApi.Services.Utilities.ResultFactories.UserManagement
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
    ///     Initializes a new instance of the <see cref="UserLookupServiceResultFactory"/> class.
    /// </remarks>
    /// <param name="parameterValidator">
    ///     The parameter validator used to validate input parameters.
    /// </param>
    public class UserLookupServiceResultFactory(IParameterValidator parameterValidator) : UserLookupServiceResultFactoryBase(parameterValidator), IUserLookupServiceResultFactory
    {
        /// <summary>
        ///     Creates a user lookup service result indicating a failure 
        ///     in the user lookup operation, along with any associated error messages.
        /// </summary>
        /// <param name="errors">An array of error messages that describe the reasons 
        ///     for the failure of the user lookup operation.</param>
        /// <returns>
        ///     A <see cref="UserLookupServiceResult"/> object indicating failure 
        ///     and containing the list of error messages.
        /// </returns>
        public override UserLookupServiceResult UserLookupOperationFailure(string[] errors)
        {
            ValidateErrors(errors);
            return new UserLookupServiceResult { Success = false, Errors = [.. errors] };
        }

        /// <summary>
        ///     Creates a user lookup service result indicating a successful 
        ///     user lookup operation, including the details of the found user.
        /// </summary>
        /// <param name="user">
        ///     The user entity that was found during the lookup.
        /// </param>
        /// <returns>
        ///     A <see cref="UserLookupServiceResult"/> object indicating success 
        ///     and containing the found user.
        /// </returns>
        public override UserLookupServiceResult UserLookupOperationSuccess(User user)
        {
            _parameterValidator.ValidateObjectNotNull(user, nameof(user));
            return new UserLookupServiceResult { Success = true, UserFound = user };
        }
    }
}
