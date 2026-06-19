using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Authorization;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.RequestModels.UserManagement;
using IdentityServiceApi.Models.ServiceResultModels.Shared;
using Microsoft.AspNetCore.Identity;

namespace IdentityServiceApi.Services.UserManagement
{
    /// <summary>
    ///     Service responsible for interacting with password-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class PasswordService : IPasswordService
    {
        private readonly UserManager<User> _userManager;
        private readonly IPasswordHistoryService _historyService;
        private readonly IPermissionService _permissionService;
        private readonly IParameterValidator _parameterValidator;
        private readonly IServiceResultFactory _serviceResultFactory;
        private readonly IUserLookupService _userLookupService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PasswordService"/> class.
        /// </summary>
        /// <param name="userManager">
        ///     The user manager used for managing user-related operations.
        /// </param>
        /// <param name="historyService">
        ///     The service responsible for managing password history.
        /// </param>
        /// <param name="permissionService">
        ///     The service used for validating user permissions.
        /// </param>
        /// <param name="parameterValidator">
        ///     The parameter validator service used for defense checking service parameters.
        /// </param>
        /// <param name="serviceResultFactory">
        ///     The service used for creating the result objects being returned in operations.
        /// </param>
        /// <param name="userLookupService">'
        ///     The service used for looking up users in the system.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when any of the parameters are null.
        /// </exception>
        public PasswordService(UserManager<User> userManager, IPasswordHistoryService historyService, IPermissionService permissionService, IParameterValidator parameterValidator, IServiceResultFactory serviceResultFactory, IUserLookupService userLookupService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
            _serviceResultFactory = serviceResultFactory ?? throw new ArgumentNullException(nameof(serviceResultFactory));
            _userLookupService = userLookupService ?? throw new ArgumentNullException(nameof(userLookupService));
        }

        /// <summary>
        ///     Asynchronously sets a password for a user in the database based on the provided ID.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user whose password is being set.
        /// </param>
        /// <param name="request">
        ///     A model object containing the password and confirmed password.
        /// </param>
        /// <returns>
        ///     A <see cref="ServiceResult"/> indicating the outcome of the password setting operation:
        ///     - If successful, Success is set to true.
        ///     - If the user ID cannot be found, an error message is provided.
        ///     - If an error occurs during setup, an error message is returned.
        /// </returns>
        public async Task<ServiceResult> SetPasswordAsync(string id, SetPasswordRequest request)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            _parameterValidator.ValidateObjectNotNull(request, nameof(request));
            _parameterValidator.ValidateNotNullOrEmpty(request.Password, nameof(request.Password));
            _parameterValidator.ValidateNotNullOrEmpty(request.PasswordConfirmed, nameof(request.PasswordConfirmed));

            if (!request.PasswordConfirmed.Equals(request.Password))
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Password.Mismatch });
            }

            var userLookupResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return _serviceResultFactory.GeneralOperationFailure(userLookupResult.Errors.ToArray());
            }

            var user = userLookupResult.UserFound;

            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Password.AlreadySet });
            }

            var result = await _userManager.AddPasswordAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return _serviceResultFactory.GeneralOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }

            await CreatePasswordHistoryAsync(user);
            return _serviceResultFactory.GeneralOperationSuccess();
        }

        /// <summary>
        ///     Asynchronously updates the password for a user in the database based on the provided ID.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user whose password is being updated.
        /// </param>
        /// <param name="request">
        ///     A model object containing the current password and the new password.
        /// </param>
        /// <returns>
        ///     A <see cref="ServiceResult"/> indicating the outcome of the password update operation:
        ///     - If successful, Success is set to true.
        ///     - If the user ID cannot be found or the current password is invalid, an error message is provided.
        ///     - If an error occurs during the update, an error message is returned.
        /// </returns>
        public async Task<ServiceResult> UpdatePasswordAsync(string id, UpdatePasswordRequest request)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            _parameterValidator.ValidateObjectNotNull(request, nameof(request));
            _parameterValidator.ValidateNotNullOrEmpty(request.CurrentPassword, nameof(request.CurrentPassword));
            _parameterValidator.ValidateNotNullOrEmpty(request.NewPassword, nameof(request.NewPassword));

            var permissionResult = await _permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return _serviceResultFactory.GeneralOperationFailure(permissionResult.Errors.ToArray());
            }

            var userLookupResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Password.InvalidCredentials });
            }

            var user = userLookupResult.UserFound;

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Password.InvalidCredentials });
            }

            var passwordIsValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!passwordIsValid)
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Password.InvalidCredentials });
            }

            // Send password to be checked against users history for re-use errors
            var isPasswordReused = await IsPasswordReusedAsync(user.Id, request.NewPassword);
            if (isPasswordReused)
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Password.CannotReuse });
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                return _serviceResultFactory.GeneralOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }

            await CreatePasswordHistoryAsync(user);
            return _serviceResultFactory.GeneralOperationSuccess();
        }

        private async Task CreatePasswordHistoryAsync(User user)
        {
            var passwordHistoryRequest = new StorePasswordHistoryRequest
            {
                UserId = user.Id,
                PasswordHash = user.PasswordHash,
            };

            await _historyService.AddPasswordHistoryAsync(passwordHistoryRequest);
        }

        private async Task<bool> IsPasswordReusedAsync(string userId, string password)
        {
            var request = new SearchPasswordHistoryRequest
            {
                UserId = userId,
                Password = password
            };

            return await _historyService.FindPasswordHashAsync(request);
        }
    }
}
