using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Features.Authorization.Models;
using IdentityServiceApi.Shared.Results;
using IdentityServiceApi.Shared.Utilities;

namespace IdentityServiceApi.Features.Authorization.Services
{
    /// <summary>
    ///     Factory class for creating role service results, used for both successful and failed role operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    ///     @Updated: 2026
    /// </remarks>
    public class RoleResultFactory(IParameterValidator parameterValidator) : RoleResultFactoryBase(parameterValidator), IRoleResultFactory
    {
        /// <summary>
        ///     Creates a failed role service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <param name="errorType">
        ///     An <see cref="ErrorType"/> indicating the type of error that occurred during the role operation.
        /// </param>
        /// <returns>
        ///     A <see cref="RoleResult"/> indicating failure along with the provided errors.
        /// </returns>
        public override RoleResult RoleOperationFailure(string[] errors, ErrorType errorType)
        {
            ValidateErrors(errors);
            return new RoleResult { Success = false, Errors = [.. errors], ErrorType = errorType };
        }

        /// <summary>
        ///     Creates a successful role operation result with a role DTO.
        /// </summary>
        /// <param name="role">
        ///     The <see cref="RoleDTO"/> representing the role information.
        /// </param>
        /// <returns>
        ///     A <see cref="RoleResult"/> containing the success status and role data.
        /// </returns>
        public override RoleResult RoleOperationSuccess(RoleDTO role)
        {
            parameterValidator.ValidateObjectNotNull(role, nameof(role));
            return new RoleResult { Success = true, Role = role };
        }
    }
}
