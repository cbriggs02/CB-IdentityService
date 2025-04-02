using AutoMapper;
using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.Logging;
using IdentityServiceApi.Interfaces.Utilities;

namespace IdentityServiceApi.Services.Logging.AbstractClasses
{
    /// <summary>
    ///     An abstract base class for logging performance-related metrics, specifically slow performance events.
    ///     This class extends <see cref="AuditLoggerService"/> and implements <see cref="IPerformanceLoggerService"/>.
    ///     It captures slow performance events by logging response times that exceed a certain threshold.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public abstract class PerformanceLoggerServiceBase : AuditLoggerService, IPerformanceLoggerService
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PerformanceLoggerServiceBase"/> class.
        ///     This constructor requires the application context, parameter validation service, 
        ///     service result factory, and mapper.
        /// </summary>
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
        protected PerformanceLoggerServiceBase(ApplicationDbContext context, IParameterValidator parameterValidator, IServiceResultFactory serviceResultFactory, IMapper mapper) : base(context, parameterValidator, serviceResultFactory, mapper)
        {
        }

        /// <summary>
        ///     Logs an event indicating that a performance response time has exceeded a specified threshold,
        ///     typically identifying slow performance.
        /// </summary>
        /// <param name="responseTime">
        ///     The response time that is considered slow. This value is typically measured in milliseconds.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the slow performance event.
        /// </returns>
        public abstract Task LogSlowPerformanceAsync(long responseTime);
    }
}
