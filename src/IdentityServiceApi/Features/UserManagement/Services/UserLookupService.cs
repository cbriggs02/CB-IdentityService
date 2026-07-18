using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Results;
using IdentityServiceApi.Shared.Utilities;
using Microsoft.AspNetCore.Identity;

namespace IdentityServiceApi.Features.UserManagement.Services
{
    /// <summary>
    ///     Provides services for looking up user information within the application. 
    ///     This service interacts with the underlying user management system to 
    ///     retrieve user data based on specified criteria and can be used across 
    ///     multiple modules in the application.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class UserLookupService(UserManager<User> userManager, IUserLookupResultFactory userLookupServiceResultFactory, IParameterValidator parameterValidator) : IUserLookupService
    {
        /// <summary>
        ///     Asynchronously retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to be retrieved.
        /// </param>
        /// <returns>
        ///     A User lookup service result representing the asynchronous operation, 
        ///     containing the result of the user lookup which includes the user information 
        ///     if found, or error details if not.
        /// </returns>
        public async Task<UserLookupResult> FindUserByIdAsync(string id)
        {
            parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            var user = await userManager.FindByIdAsync(id);
            return HandleLookupResult(user);
        }

        /// <summary>
        ///     Asynchronously retrieves a user by their user name.
        /// </summary>
        /// <param name="userName">
        ///     The username of the user to find.
        /// </param>
        /// <returns>
        ///     A User lookup service result representing the asynchronous operation, 
        ///     containing the result of the user lookup which includes the user information 
        ///     if found, or error details if not.
        /// </returns>
        public async Task<UserLookupResult> FindUserByUsernameAsync(string userName)
        {
            parameterValidator.ValidateNotNullOrEmpty(userName, nameof(userName));
            var user = await userManager.FindByNameAsync(userName);
            return HandleLookupResult(user);
        }

        private UserLookupResult HandleLookupResult(User? user)
        {
            if (user == null)
            {
                return userLookupServiceResultFactory.UserLookupOperationFailure([ErrorMessages.User.NotFound], ErrorType.NotFound);
            }
            return userLookupServiceResultFactory.UserLookupOperationSuccess(user);
        }
    }
}
