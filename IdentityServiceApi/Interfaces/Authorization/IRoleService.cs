using IdentityServiceApi.Models.ServiceResultModels.Authorization;
using IdentityServiceApi.Models.ServiceResultModels.Shared;

namespace IdentityServiceApi.Interfaces.Authorization
{
    /// <summary>
    ///     Interface defining the contract for a service responsible for role-related operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public interface IRoleService
    {
        /// <summary>
        ///     Asynchronously retrieves a list of all roles available in the system.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation, containing a collection of role data transfer objects.
        /// </returns>
        Task<RoleServiceListResult> GetRolesAsync();

        /// <summary>
        ///     Asynchronously assigns a specified role to a user identified by their ID.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to whom the role will be assigned.
        /// </param>
        /// <param name="roleName">
        ///     The name of the role that is being assigned to the user.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a result object indicating the outcome of the role assignment.
        /// </returns>
        Task<ServiceResult> AssignRoleAsync(string id, string roleName);

        /// <summary>
        ///     Asynchronously removes a specified role to a user identified by their ID.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to whom the role will be removed.
        /// </param>
        /// <param name="roleName">
        ///     The name of the role that is being removed to the user.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a result object indicating the outcome of the role removal.
        /// </returns>
        Task<ServiceResult> RemoveRoleAsync(string id, string roleName);
    }
}
