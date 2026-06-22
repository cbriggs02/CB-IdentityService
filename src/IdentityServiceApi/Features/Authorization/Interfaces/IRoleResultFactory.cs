using IdentityServiceApi.Features.Authorization.Models;
using IdentityServiceApi.Shared.Results;

namespace IdentityServiceApi.Features.Authorization.Interfaces
{
    /// <summary>
    ///     Interface for creating service results related to role operations.
    ///     This interface defines methods for creating both success and failure results
    ///     for role operations, including handling the role DTO for successful role operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    ///     @Updated: 2026
    /// </remarks>
    public interface IRoleResultFactory : IResultFactory
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
        RoleResult RoleOperationFailure(string[] errors, ErrorType errorType);

        /// <summary>
        ///     Creates a successful role service result with a role DTO.
        /// </summary>
        /// <param name="role">
        ///     The role DTO generated upon successful role operation.
        /// </param>
        /// <returns>
        ///     A <see cref="RoleResult"/> containing the success status and the role DTO.
        /// </returns>
        RoleResult RoleOperationSuccess(RoleDTO role);
    }
}
