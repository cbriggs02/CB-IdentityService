using IdentityServiceApi.Constants;
using IdentityServiceApi.Enums;
using IdentityServiceApi.Interfaces.Authorization;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.ServiceResultModels.Shared;

namespace IdentityServiceApi.Services.Authorization
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
    public class PermissionService(IAuthorizationService authService, IParameterValidator parameterValidator, IServiceResultFactory serviceResultFactory, ILoggerService loggerService) : IPermissionService
    {
        private readonly IAuthorizationService _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        private readonly IParameterValidator _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
        private readonly IServiceResultFactory _serviceResultFactory = serviceResultFactory ?? throw new ArgumentNullException(nameof(serviceResultFactory));
        private readonly ILoggerService _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));

        /// <summary>
        ///     Asynchronously validates the permissions of a user identified by the specified ID.
        /// </summary>
        /// <param name="id">
        ///     The unique ID of the user whose permissions are to be validated.
        /// </param>
        /// <returns>
        ///     A <see cref="ServiceResult"/> indicating the outcome of the validation:
        ///     - If the user has the necessary permissions, returns a result with Success set to true.
        ///     - If the user lacks the required permissions, returns a result with Success set to false 
        ///       and an appropriate error message.
        /// </returns>
        public async Task<ServiceResult> ValidatePermissionsAsync(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            // Use the auth service to check permissions
            bool hasPermission = await _authService.ValidatePermissionAsync(id);
            if (!hasPermission)
            {
                _loggerService.LogData(LogLevel.Warning, LogSource.PermissionService, $"User with ID {id} does not have the required permissions.");
                return _serviceResultFactory.GeneralOperationFailure([ErrorMessages.Authorization.Forbidden]);
            }

            return _serviceResultFactory.GeneralOperationSuccess();
        }
    }
}
