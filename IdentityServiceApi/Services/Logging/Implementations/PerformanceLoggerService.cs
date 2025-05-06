using AutoMapper;
using IdentityServiceApi.Constants;
using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.Authentication;
using IdentityServiceApi.Interfaces.Logging;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Services.Logging.AbstractClasses;

namespace IdentityServiceApi.Services.Logging.Implementations
{
    /// <summary>
    ///     A service for logging slow performance events, extending the functionality of 
    ///     <see  cref="PerformanceLoggerServiceBase"/>. This service logs actions that have 
    ///     a response time exceeding a predefined threshold, indicating slow performance.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class PerformanceLoggerService : PerformanceLoggerServiceBase, IPerformanceLoggerService
    {
        private readonly IUserContextService _userContextService;
        private readonly ILoggingValidator _loggingValidator;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PerformanceLoggerService"/> class.
        /// </summary>
        /// <param name="userContextService">
        ///     The service for handling user context, including retrieving user-related information and request details.
        /// </param>
        /// <param name="loggingValidator">
        ///     The service used for validating context data used in logs.
        /// </param>
        /// <param name="context">
        ///     The application database context used for logging purposes.
        /// </param>
        /// <param name="parameterValidator">
        ///     An instance of <see cref="IParameterValidator"/> used for validating input parameters.
        /// </param>
        /// <param name="auditLogServiceResultFactory">
        ///     A factory used to create standardized service result objects.
        /// </param>
        /// <param name="mapper">
        ///     An instance of AutoMapper used for object mapping.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if <paramref name="userContextService"/> is null.
        /// </exception>
        public PerformanceLoggerService(IUserContextService userContextService, ILoggingValidator loggingValidator, ApplicationDbContext context, IParameterValidator parameterValidator, IAuditLoggerServiceResultFactory auditLogServiceResultFactory, IMapper mapper) : base(context, parameterValidator, auditLogServiceResultFactory, mapper)
        {
            _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
            _loggingValidator = loggingValidator ?? throw new ArgumentNullException(nameof(loggingValidator));
        }

        /// <summary>
        ///     Logs an event indicating slow performance, capturing the response time, request path, user information, 
        ///     and IP address. This method helps track performance issues by logging actions that take longer than expected.
        /// <remarks>
        ///     This method is typically used to monitor performance bottlenecks and capture events where the response 
        ///     time exceeds acceptable limits for user actions in the application.
        /// </remarks>
        /// <param name="responseTime">
        /// The response time (in milliseconds) for the action being logged. This value must be greater than zero.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the slow performance event.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if the provided response time is less than or equal to zero.
        /// </exception>
        public override async Task LogSlowPerformanceAsync(long responseTime)
        {
            if (responseTime <= 0)
            {
                throw new ArgumentException(ErrorMessages.AuditLog.PerformanceLog.InvalidResponseTime);
            }

            var principal = _userContextService.GetClaimsPrincipal();
            string? currentUserId = null;

            if (principal != null)
            {
                currentUserId = _userContextService.GetUserId(principal);
            }

            var ipAddress = _userContextService.GetAddress()?.ToString() ?? "Unknown";
            var requestPath = _userContextService.GetRequestPath() ?? "Unknown Path";

            _loggingValidator.ValidateContextData(ipAddress, nameof(ipAddress));
            _loggingValidator.ValidateContextData(requestPath, nameof(requestPath));

            var log = new AuditLog
            {
                Action = AuditAction.SlowPerformance,
                UserId = currentUserId,
                TimeStamp = DateTime.UtcNow,
                Details = $"Action: {requestPath}, Response Time: {responseTime} ms",
                IpAddress = ipAddress
            };

            await AddLog(log);
        }
    }
}
