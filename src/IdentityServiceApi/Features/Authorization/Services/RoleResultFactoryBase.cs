using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Features.Authorization.Models;
using IdentityServiceApi.Shared.Results;

namespace IdentityServiceApi.Features.Authorization.Services
{
    /// <summary>
    ///     Base class for creating role service results, used for both successful and failed role operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    ///     @Updated: 2026
    /// </remarks>>
    public abstract class RoleResultFactoryBase : ResultFactory, IRoleResultFactory
    {
        /// <summary>
        ///     Creates a failed role operation service result with specified errors.
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
        public abstract RoleResult RoleOperationFailure(string[] errors, ErrorType errorType);

        /// <summary>
        ///     Creates a successful role operation service result with the specified role data.
        /// </summary>
        /// <param name="role">
        ///     The <see cref="RoleDTO"/> representing the successfully found role.
        /// </param>
        /// <returns>
        ///     A <see cref="RoleResult"/> containing the success status and the role data.
        /// </returns>
        public abstract RoleResult RoleOperationSuccess(RoleDTO role);
    }
}
