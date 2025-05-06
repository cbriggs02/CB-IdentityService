using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.ServiceResultModels.Logging;
using IdentityServiceApi.Services.Utilities.ResultFactories.Common;

namespace IdentityServiceApi.Services.Utilities.ResultFactories.BaseClasses
{
    /// <summary>
    ///     Base class for creating audit logger service results, used for both successful and failed audit logger operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public abstract class AuditLoggerServiceResultFactoryBase : ServiceResultFactory, IAuditLoggerServiceResultFactory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AuditLoggerServiceResultFactoryBase"/> class.
        /// </summary>
        /// <param name="parameterValidator">
        ///     The validator used to verify input parameters before creating results.
        /// </param>
        protected AuditLoggerServiceResultFactoryBase(IParameterValidator parameterValidator) : base(parameterValidator)
        {
        }

        /// <summary>
        ///     Creates a failed login service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="LoginServiceResult"/> indicating failure along with the provided errors.
        /// </returns>
        public abstract AuditLogServiceResult AuditLoggerOperationFailure(string[] errors);

        /// <summary>
        ///    Creates a successful audit logger service result with a audit log entry.
        /// </summary>
        /// <param name="auditLog">
        ///     The <see cref="AuditLogDTO"/> instance containing the logged audit data.
        /// </param>
        /// <returns>
        ///     An <see cref="AuditLogServiceResult"/> that encapsulates the successful operation details.
        /// </returns>
        public abstract AuditLogServiceResult AuditLoggerOperationSuccess(AuditLogDTO auditLog);
    }
}
