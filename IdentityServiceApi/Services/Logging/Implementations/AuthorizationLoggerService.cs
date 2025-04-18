using AutoMapper;
using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.Authentication;
using IdentityServiceApi.Interfaces.Logging;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Services.Logging.AbstractClasses;

namespace IdentityServiceApi.Services.Logging.Implementations
{
    /// <summary>
    ///     A concrete implementation of <see cref="AuthorizationLoggerServiceBase"/> that logs 
    ///     authorization breaches. This service logs attempts to access unauthorized resources 
    ///     and tracks information such as the user ID, 
    ///     request path, and IP address.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class AuthorizationLoggerService : AuthorizationLoggerServiceBase, IAuthorizationLoggerService
    {
        private readonly IUserContextService _userContextService;
        private readonly ILoggingValidator _loggingValidator;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthorizationLoggerService"/> class.
        ///     This constructor takes the necessary dependencies and initializes the service.
        /// </summary>
        /// <param name="userContextService">
        ///     An instance of <see cref="IUserContextService"/> that provides user-specific context information 
        ///     such as user ID, request path, and IP address.
        /// </param>
        /// <param name="loggingValidator">
        ///     The service used for validating context data used in logs.
        /// </param>
        /// <param name="context">
        ///     The application database context used to interact with the database.
        /// </param>
        /// <param name="parameterValidator">
        ///     An instance of <see cref="IParameterValidator"/> used for validating input parameters.
        /// </param>
        /// <param name="serviceResultFactory">
        ///     A factory used to create standardized service result objects.
        /// </param>
        /// <param name="mapper">
        ///     An instance of AutoMapper used to map objects.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="userContextService"/> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException"></exception>
        public AuthorizationLoggerService(IUserContextService userContextService, ILoggingValidator loggingValidator, ApplicationDbContext context, IParameterValidator parameterValidator, IServiceResultFactory serviceResultFactory, IMapper mapper) : base(context, parameterValidator, serviceResultFactory, mapper)
        {
            _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
            _loggingValidator = loggingValidator ?? throw new ArgumentNullException(nameof(loggingValidator));
        }

        /// <summary>
        ///     Logs an authorization breach event, capturing details about unauthorized access attempts.
        ///     This method retrieves the current user's identity, IP address, and the requested path, 
        ///     and logs an event indicating an unauthorized access attempt.
        /// <remarks>
        ///     This method is typically used to log unauthorized access attempts for security and audit purposes.
        /// </remarks>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the authorization breach.
        /// </returns>
        public override async Task LogAuthorizationBreachAsync()
        {
            var principal = _userContextService.GetClaimsPrincipal();
            string? currentUserId = null;

            if(principal != null)
            {
                currentUserId = _userContextService.GetUserId(principal);
            }
             
            var ipAddress = _userContextService.GetAddress()?.ToString() ?? "Unknown";
            var requestPath = _userContextService.GetRequestPath() ?? "Unknown Path";

            _loggingValidator.ValidateContextData(ipAddress, nameof(ipAddress));
            _loggingValidator.ValidateContextData(requestPath, nameof(requestPath));

            var log = new AuditLog
            {
                Action = AuditAction.AuthorizationBreach,
                UserId = currentUserId,
                TimeStamp = DateTime.UtcNow,
                Details = $"Unauthorized access attempt to {requestPath}",
                IpAddress = ipAddress
            };

            await AddLog(log);
        }
    }
}
