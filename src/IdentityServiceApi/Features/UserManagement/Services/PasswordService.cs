using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Requests;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Shared.Results;
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
            parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            parameterValidator.ValidateObjectNotNull(request, nameof(request));
            parameterValidator.ValidateNotNullOrEmpty(request.Password, nameof(request.Password));
            parameterValidator.ValidateNotNullOrEmpty(request.PasswordConfirmed, nameof(request.PasswordConfirmed));

            if (!request.PasswordConfirmed.Equals(request.Password))
            {
                return serviceResultFactory.GeneralOperationFailure([ErrorMessages.Password.Mismatch], ErrorType.Validation);
            }

            var userLookupResult = await userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return serviceResultFactory.GeneralOperationFailure([.. userLookupResult.Errors], userLookupResult.ErrorType);
            }

            var user = userLookupResult.UserFound;
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                return serviceResultFactory.GeneralOperationFailure([ErrorMessages.Password.AlreadySet], ErrorType.InvalidState);
            }

            var result = await userManager.AddPasswordAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return serviceResultFactory.GeneralOperationFailure([.. result.Errors.Select(e => e.Description)], ErrorType.Validation);
            }

            LogPasswordOperation(LogLevel.Information, $"Password set for user with ID {id}");
            await CreatePasswordHistoryAsync(user);
            return serviceResultFactory.GeneralOperationSuccess();
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
            parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            parameterValidator.ValidateObjectNotNull(request, nameof(request));
            parameterValidator.ValidateNotNullOrEmpty(request.CurrentPassword, nameof(request.CurrentPassword));
            parameterValidator.ValidateNotNullOrEmpty(request.NewPassword, nameof(request.NewPassword));

            var permissionResult = await permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return serviceResultFactory.GeneralOperationFailure([.. permissionResult.Errors], permissionResult.ErrorType);
            }

            var userLookupResult = await userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return serviceResultFactory.GeneralOperationFailure([ErrorMessages.Password.InvalidCredentials], userLookupResult.ErrorType);
            }

            var user = userLookupResult.UserFound;

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                return serviceResultFactory.GeneralOperationFailure([ErrorMessages.Password.MissingHash], ErrorType.InvalidState);
            }

            var passwordIsValid = await userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!passwordIsValid)
            {
                return serviceResultFactory.GeneralOperationFailure([ErrorMessages.Password.InvalidCredentials], ErrorType.Unauthorized);
            }

            // Send password to be checked against users history for re-use errors
            var isPasswordReused = await IsPasswordReusedAsync(user.Id, request.NewPassword);
            if (isPasswordReused)
            {
                return serviceResultFactory.GeneralOperationFailure([ErrorMessages.Password.CannotReuse], ErrorType.Validation);
            }

            var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                LogPasswordOperation(LogLevel.Warning, $"Password changed failed for user with ID {id}");
                return serviceResultFactory.GeneralOperationFailure([.. result.Errors.Select(e => e.Description)], ErrorType.Validation);
            }

            LogPasswordOperation(LogLevel.Information, $"Password changed for user with ID {id}");
            await CreatePasswordHistoryAsync(user);
            return serviceResultFactory.GeneralOperationSuccess();
        }

        private async Task CreatePasswordHistoryAsync(User user)
        {
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                LogPasswordOperation(LogLevel.Warning,
                    $"Attempted to create password history for user with ID {user.Id} but no password hash was found.");
                return;
            }

            var passwordHistoryRequest = new StorePasswordHistoryRequest
            {
                UserId = user.Id,
                PasswordHash = user.PasswordHash,
            };

            await historyService.AddPasswordHistoryAsync(passwordHistoryRequest);
        }

        private async Task<bool> IsPasswordReusedAsync(string userId, string password)
        {
            var request = new SearchPasswordHistoryRequest
            {
                UserId = userId,
                Password = password
            };

            return await historyService.FindPasswordHashAsync(request);
        }

        private void LogPasswordOperation(LogLevel logLevel, string message)
        {
            var logEntry = new LogEntry
            {
                LogLevel = logLevel,
                LogSource = LogSource.PasswordService,
                Message = message
            };

            loggerService.LogData(logEntry);
        }
    }
}
