using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.ServiceResultModels.UserManagement;
using Microsoft.AspNetCore.Identity;

namespace IdentityServiceApi.Services.UserManagement
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
    /// </remarks>
    public class UserLookupService : IUserLookupService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserLookupServiceResultFactory _userLookupServiceResultFactory;
        private readonly IParameterValidator _parameterValidator;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserLookupService"/> class.
        /// </summary>
        /// <param name="userManager">
        ///     The user manager used for managing user-related operations.
        /// </param>
        /// <param name="userLookupServiceResultFactory">
        ///     The service used for creating the result objects being returned in operations.
        /// </param>
        /// <param name="parameterValidator">
        ///     The parameter validator service used for defense checking service parameters.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when any of the provided parameters are null.
        /// </exception>
        public UserLookupService(UserManager<User> userManager, IUserLookupServiceResultFactory userLookupServiceResultFactory, IParameterValidator parameterValidator)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _userLookupServiceResultFactory = userLookupServiceResultFactory ?? throw new ArgumentNullException(nameof(userLookupServiceResultFactory));
            _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
        }

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
        public async Task<UserLookupServiceResult> FindUserByIdAsync(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            var user = await _userManager.FindByIdAsync(id);
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
        public async Task<UserLookupServiceResult> FindUserByUsernameAsync(string userName)
        {
            _parameterValidator.ValidateNotNullOrEmpty(userName, nameof(userName));
            var user = await _userManager.FindByNameAsync(userName);
            return HandleLookupResult(user);
        }

        private UserLookupServiceResult HandleLookupResult(User user)
        {
            if (user == null)
            {
                return _userLookupServiceResultFactory.UserLookupOperationFailure(new[] { ErrorMessages.User.NotFound });
            }
            return _userLookupServiceResultFactory.UserLookupOperationSuccess(user);
        }
    }
}
