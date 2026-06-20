using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Requests;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Shared.ResultFactories;
using IdentityServiceApi.Shared.Utilities;
using Microsoft.AspNetCore.Identity;

namespace IdentityServiceApi.Features.UserManagement.Services
{
    /// <summary>
    ///     Service responsible for interacting with password-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class PasswordService(UserManager<User> userManager, IPasswordHistoryService historyService, IPermissionService permissionService, IParameterValidator parameterValidator, IResultFactory serviceResultFactory, IUserLookupService userLookupService, ILoggerService loggerService) : IPasswordService
    {
        private readonly UserManager<User> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IPasswordHistoryService _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        private readonly IPermissionService _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        private readonly IParameterValidator _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
        private readonly IResultFactory _serviceResultFactory = serviceResultFactory ?? throw new ArgumentNullException(nameof(serviceResultFactory));
        private readonly IUserLookupService _userLookupService = userLookupService ?? throw new ArgumentNullException(nameof(userLookupService));
        private readonly ILoggerService _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));

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
        ///     A <see cref="Result"/> indicating the outcome of the password setting operation:
        ///     - If successful, Success is set to true.
        ///     - If the user ID cannot be found, an error message is provided.
        ///     - If an error occurs during setup, an error message is returned.
        /// </returns>
        public async Task<Result> SetPasswordAsync(string id, SetPasswordRequest request)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            _parameterValidator.ValidateObjectNotNull(request, nameof(request));
            _parameterValidator.ValidateNotNullOrEmpty(request.Password, nameof(request.Password));
            _parameterValidator.ValidateNotNullOrEmpty(request.PasswordConfirmed, nameof(request.PasswordConfirmed));

            if (!request.PasswordConfirmed.Equals(request.Password))
            {
                return _serviceResultFactory.GeneralOperationFailure([ErrorMessages.Password.Mismatch]);
            }

            var userLookupResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return _serviceResultFactory.GeneralOperationFailure([.. userLookupResult.Errors]);
            }

            var user = userLookupResult.UserFound;
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                return _serviceResultFactory.GeneralOperationFailure([ErrorMessages.Password.AlreadySet]);
            }

            var result = await _userManager.AddPasswordAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return _serviceResultFactory.GeneralOperationFailure([.. result.Errors.Select(e => e.Description)]);
            }

            LogPasswordOperation(LogLevel.Information, $"Password set for user with ID {id}");
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
        ///     A <see cref="Result"/> indicating the outcome of the password update operation:
        ///     - If successful, Success is set to true.
        ///     - If the user ID cannot be found or the current password is invalid, an error message is provided.
        ///     - If an error occurs during the update, an error message is returned.
        /// </returns>
        public async Task<Result> UpdatePasswordAsync(string id, UpdatePasswordRequest request)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            _parameterValidator.ValidateObjectNotNull(request, nameof(request));
            _parameterValidator.ValidateNotNullOrEmpty(request.CurrentPassword, nameof(request.CurrentPassword));
            _parameterValidator.ValidateNotNullOrEmpty(request.NewPassword, nameof(request.NewPassword));

            var permissionResult = await _permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return _serviceResultFactory.GeneralOperationFailure([.. permissionResult.Errors]);
            }

            var userLookupResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return _serviceResultFactory.GeneralOperationFailure([ErrorMessages.Password.InvalidCredentials]);
            }

            var user = userLookupResult.UserFound;

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                return _serviceResultFactory.GeneralOperationFailure([ErrorMessages.Password.InvalidCredentials]);
            }

            var passwordIsValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!passwordIsValid)
            {
                return _serviceResultFactory.GeneralOperationFailure([ErrorMessages.Password.InvalidCredentials]);
            }

            // Send password to be checked against users history for re-use errors
            var isPasswordReused = await IsPasswordReusedAsync(user.Id, request.NewPassword);
            if (isPasswordReused)
            {
                return _serviceResultFactory.GeneralOperationFailure([ErrorMessages.Password.CannotReuse]);
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                LogPasswordOperation(LogLevel.Warning, $"Password changed failed for user with ID {id}");
                return _serviceResultFactory.GeneralOperationFailure([.. result.Errors.Select(e => e.Description)]);
            }

            LogPasswordOperation(LogLevel.Information, $"Password changed for user with ID {id}");
            await CreatePasswordHistoryAsync(user);
            return _serviceResultFactory.GeneralOperationSuccess();
        }

        private async Task CreatePasswordHistoryAsync(User user)
        {
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                LogPasswordOperation(LogLevel.Warning, $"Attempted to create password history for user with ID {user.Id} but no password hash was found.");
                return;
            }

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

        private void LogPasswordOperation(LogLevel logLevel, string message)
        {
            var logEntry = new LogEntry
            {
                LogLevel = logLevel,
                LogSource = LogSource.PasswordService,
                Message = message
            };

            _loggerService.LogData(logEntry);
        }
    }
}
