using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Shared.Results;
using IdentityServiceApi.Shared.Utilities;

namespace IdentityServiceApi.Features.Authorization.Services
{
    /// <summary>
    ///     Service responsible for interacting with authorization-related data and business logic.
    ///     This service encapsulates the interaction between other services and the auth service.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class PermissionService(IAuthorizationService authService, IParameterValidator parameterValidator, IResultFactory serviceResultFactory, ILoggerService loggerService) : IPermissionService
    {
        /// <summary>
        ///     Asynchronously validates the permissions of a user identified by the specified ID.
        /// </summary>
        /// <param name="id">
        ///     The unique ID of the user whose permissions are to be validated.
        /// </param>
        /// <returns>
        ///     A <see cref="Result"/> indicating the outcome of the validation:
        ///     - If the user has the necessary permissions, returns a result with Success set to true.
        ///     - If the user lacks the required permissions, returns a result with Success set to false 
        ///       and an appropriate error message.
        /// </returns>
        public async Task<Result> ValidatePermissionsAsync(string id)
        {
            parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            // Use the auth service to check permissions
            bool hasPermission = await authService.ValidatePermissionAsync(id);
            if (!hasPermission)
            {
                LogPermissionValidationFailure(id);
                return serviceResultFactory.GeneralOperationFailure([ErrorMessages.Authorization.Forbidden], ErrorType.Forbidden);
            }

            return serviceResultFactory.GeneralOperationSuccess();
        }

        private void LogPermissionValidationFailure(string id)
        {
            var LogEntry = new LogEntry
            {
                LogLevel = LogLevel.Warning,
                LogSource = LogSource.PermissionService,
                Message = $"User with ID {id} does not have the required permissions."
            };

            loggerService.LogData(LogEntry);
        }
    }
}
