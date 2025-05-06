using AutoMapper;
using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.Logging;
using IdentityServiceApi.Interfaces.Utilities;

namespace IdentityServiceApi.Services.Logging.AbstractClasses
{
    /// <summary>
    ///     An abstract base class that provides functionality for logging authorization breaches.
    ///     This class extends the <see cref="AuditLoggerService"/> and implements 
    ///     <see cref="IAuthorizationLoggerService"/>. It serves as a foundation for services that 
    ///     handle authorization-related logging activities.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public abstract class AuthorizationLoggerServiceBase : AuditLoggerService, IAuthorizationLoggerService
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthorizationLoggerServiceBase"/> class.
        ///     This constructor initializes the required dependencies for logging services.
        /// </summary>
        /// <param name="context">
        ///     The application database context, used to interact with the database.
        /// </param>
        /// <param name="parameterValidator">
        ///     An object responsible for validating parameters and input data.
        /// </param>
        /// <param name="auditLogServiceResultFactory">
        ///     A factory for creating audit log service result objects.
        /// </param>
        /// <param name="mapper">
        ///     An AutoMapper instance used for object-to-object mapping.
        /// </param>
        protected AuthorizationLoggerServiceBase(ApplicationDbContext context, IParameterValidator parameterValidator, IAuditLoggerServiceResultFactory auditLogServiceResultFactory, IMapper mapper) : base(context, parameterValidator, auditLogServiceResultFactory, mapper)
        {
        }

        /// <summary>
        ///     Logs an authorization breach event. 
        ///     This method is abstract and should be implemented by a derived class to perform 
        ///     the specific logic for logging authorization breaches.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation of logging the authorization breach.
        /// </returns>
        public abstract Task LogAuthorizationBreachAsync();
    }
}
