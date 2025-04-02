using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Authorization;
using IdentityServiceApi.Interfaces.Logging;
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
    /// </remarks>
    public class PermissionService : IPermissionService
    {
        private readonly IAuthorizationService _authService;
        private readonly ILoggerService _loggerService;
        private readonly IParameterValidator _parameterValidator;
        private readonly IServiceResultFactory _serviceResultFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PermissionService"/> class.
        /// </summary>
        /// <param name="authService">
        ///     The authorization service responsible for managing user permissions.
        /// </param>
        /// <param name="loggerService">
        ///     log service used for logging authorization breaches with audit logger service.
        /// </param>
        /// <param name="parameterValidator">
        ///     The parameter validator service used for defense checking service parameters.
        /// </param>
        /// <param name="serviceResultFactory">
        ///     The service used for creating the result objects being returned in operations.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="authService"/> is null.
        /// </exception>
        public PermissionService(IAuthorizationService authService, ILoggerService loggerService, IParameterValidator parameterValidator, IServiceResultFactory serviceResultFactory)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
            _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
            _serviceResultFactory = serviceResultFactory ?? throw new ArgumentNullException(nameof(serviceResultFactory));
        }

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
                await _loggerService.LogAuthorizationBreachAsync();
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Authorization.Forbidden });
            }

            return _serviceResultFactory.GeneralOperationSuccess();
        }
    }
}
