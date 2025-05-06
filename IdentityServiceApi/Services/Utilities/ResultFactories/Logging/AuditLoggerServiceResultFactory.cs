using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.ServiceResultModels.Logging;
using IdentityServiceApi.Services.Utilities.ResultFactories.BaseClasses;

namespace IdentityServiceApi.Services.Utilities.ResultFactories.Logging
{
    /// <summary>
    ///     Factory class for creating audit logging service results, 
    ///     used for both successful and failed audit logging operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    /// </remarks>
    public class AuditLoggerServiceResultFactory : AuditLoggerServiceResultFactoryBase, IAuditLoggerServiceResultFactory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AuditLoggerServiceResultFactory"/> class.
        /// </summary>
        /// <param name="parameterValidator">
        ///     The parameter validator used to validate input parameters.
        /// </param>
        public AuditLoggerServiceResultFactory(IParameterValidator parameterValidator) : base(parameterValidator)
        {
        }

        /// <summary>
        ///     Creates an audit logging service result indicating a failure 
        ///     in the audit operation, along with any associated error messages.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages that describe the reasons for the failure 
        ///     of the audit logging operation.
        /// </param>
        /// <returns>
        ///     An <see cref="AuditLogServiceResult"/> object indicating failure 
        ///     and containing the list of error messages.
        /// </returns>
        public override AuditLogServiceResult AuditLoggerOperationFailure(string[] errors)
        {
            ValidateErrors(errors);
            return new AuditLogServiceResult { Success = false, Errors = errors.ToList() };
        }

        /// <summary>
        ///     Creates an audit logging service result indicating a successful 
        ///     audit operation, including the details of the audit log entry.
        /// </summary>
        /// <param name="auditLog">
        ///     The <see cref="AuditLogDTO"/> object containing the details of the audit log entry.
        /// </param>
        /// <returns>
        ///     An <see cref="AuditLogServiceResult"/> object indicating success 
        ///     and containing the audit log data.
        /// </returns>
        public override AuditLogServiceResult AuditLoggerOperationSuccess(AuditLogDTO auditLog)
        {
            _parameterValidator.ValidateObjectNotNull(auditLog, nameof(auditLog));
            _parameterValidator.ValidateNotNullOrEmpty(auditLog.Details, nameof(auditLog.Details));
            _parameterValidator.ValidateNotNullOrEmpty(auditLog.IpAddress, nameof(auditLog.IpAddress));

            return new AuditLogServiceResult { Success = true, AuditLog = auditLog };
        }
    }
}
