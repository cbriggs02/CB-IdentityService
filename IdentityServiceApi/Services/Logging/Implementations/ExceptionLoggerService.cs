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
    ///     A service for logging exceptions. This class extends the <see cref="ExceptionLoggerServiceBase"/> and 
    ///     implements the <see cref="IExceptionLoggerService"/> interface. It captures exception details, including 
    ///     the user ID and IP address, and logs them into the system for auditing purposes.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class ExceptionLoggerService : ExceptionLoggerServiceBase, IExceptionLoggerService
    {
        private readonly IUserContextService _userContextService;
        private readonly ILoggingValidator _loggingValidator;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExceptionLoggerService"/> class.
        /// </summary>
        /// <param name="userContextService">
        ///     The service used to retrieve user context, such as user ID, claims principal, and IP address.
        /// </param>
        /// <param name="loggingValidator">
        ///     The service used for validating context data used in logs and input parameters.
        /// </param>
        /// <param name="context">
        ///     The application database context used for interaction with the database for logging purposes.
        /// </param>
        /// <param name="parameterValidator">
        ///     An instance of <see cref="IParameterValidator"/> used for validating input parameters.
        /// </param>
        /// <param name="serviceResultFactory">
        ///     A factory used to create standardized service result objects.
        /// </param>
        /// <param name="mapper">
        ///     An instance of AutoMapper used for mapping objects during logging.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if <paramref name="userContextService"/> is null.
        /// </exception>
        public ExceptionLoggerService(IUserContextService userContextService, ILoggingValidator loggingValidator, ApplicationDbContext context, IParameterValidator parameterValidator, IServiceResultFactory serviceResultFactory, IMapper mapper) : base(context, parameterValidator, serviceResultFactory, mapper)
        {
            _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
            _loggingValidator = loggingValidator ?? throw new ArgumentNullException(nameof(loggingValidator));
        }

        /// <summary>
        ///     Logs an exception event, capturing details about the exception and the context in which it occurred.
        ///     This method retrieves the current user's identity, IP address, and logs the exception details, 
        ///     providing an audit trail for error occurrences in the application.
        /// <remarks>
        ///     This method is typically used to log unexpected exceptions for troubleshooting and security audit purposes.
        /// </remarks>
        /// <param name="exception">
        ///     The exception to log. This parameter is required and cannot be null.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the exception.
        /// </returns>
        public override async Task LogExceptionAsync(Exception exception)
        {
            _loggingValidator.ValidateObjectNotNull(exception, nameof(exception));

            var principal = _userContextService.GetClaimsPrincipal();
            var currentUserId = _userContextService.GetUserId(principal) ?? "Anonymous";
            var ipAddress = _userContextService.GetAddress()?.ToString() ?? "Unknown";

            _loggingValidator.ValidateContextData(currentUserId, nameof(currentUserId));
            _loggingValidator.ValidateContextData(ipAddress, nameof(ipAddress));

            var log = new AuditLog
            {
                Action = AuditAction.Exception,
                UserId = currentUserId,
                TimeStamp = DateTime.UtcNow,
                Details = exception.ToString(),
                IpAddress = ipAddress
            };

            await AddLog(log);
        }
    }
}
