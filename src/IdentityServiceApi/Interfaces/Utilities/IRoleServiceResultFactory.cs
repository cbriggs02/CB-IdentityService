using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.ServiceResultModels.Authorization;

namespace IdentityServiceApi.Interfaces.Utilities
{
    /// <summary>
    ///     Interface for creating service results related to role operations.
    ///     This interface defines methods for creating both success and failure results
    ///     for role operations, including handling the role DTO for successful role operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public interface IRoleServiceResultFactory : IServiceResultFactory
    {
        /// <summary>
        ///     Creates a failed role service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="RoleServiceResult"/> indicating failure along with the provided errors.
        /// </returns>
        RoleServiceResult RoleOperationFailure(string[] errors);

        /// <summary>
        ///     Creates a successful role service result with a role DTO.
        /// </summary>
        /// <param name="role">
        ///     The role DTO generated upon successful role operation.
        /// </param>
        /// <returns>
        ///     A <see cref="RoleServiceResult"/> containing the success status and the role DTO.
        /// </returns>
        RoleServiceResult RoleOperationSuccess(RoleDTO role);
    }
}
