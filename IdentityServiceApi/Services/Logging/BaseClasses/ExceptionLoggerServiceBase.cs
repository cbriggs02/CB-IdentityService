using AutoMapper;
using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.Logging;
using IdentityServiceApi.Interfaces.Utilities;

namespace IdentityServiceApi.Services.Logging.AbstractClasses
{
    /// <summary>
    ///     An abstract base class for logging exceptions. This class extends the <see cref="AuditLoggerService"/> 
    ///     and implements the <see cref="IExceptionLoggerService"/> interface, providing a foundation for 
    ///     logging exceptions in the system. Concrete implementations will implement the <see cref="LogExceptionAsync"/>
    ///     method to handle the actual exception logging logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public abstract class ExceptionLoggerServiceBase : AuditLoggerService, IExceptionLoggerService
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ExceptionLoggerServiceBase"/> class.
        ///     This constructor is used to initialize the necessary dependencies for logging exceptions, 
        ///     including the application context, parameter validation, and service result factory.
        /// </summary>
        /// <param name="context">
        ///     The application database context used to interact with the database for logging purposes.
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
        protected ExceptionLoggerServiceBase(ApplicationDbContext context, IParameterValidator parameterValidator, IServiceResultFactory serviceResultFactory, IMapper mapper) : base(context, parameterValidator, serviceResultFactory, mapper)
        {
        }

        /// <summary>
        ///     Logs an exception in the system. This method is an abstract method, and concrete implementations 
        ///     must provide the actual logic for handling the exception logging. It is intended to capture relevant 
        ///     details about the exception and record them in the audit log or another logging mechanism.
        /// </summary>
        /// <param name="exception">
        ///     The exception that needs to be logged. This could be any exception that occurred within the application.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the exception.
        /// </returns>
        public abstract Task LogExceptionAsync(Exception exception);
    }
}
