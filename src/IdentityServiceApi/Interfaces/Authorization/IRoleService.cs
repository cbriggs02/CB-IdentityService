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
        ///     Asynchronously retrieves a role by its unique identifier.
        /// </summary>
        /// <param name="roleId">
        ///     The unique identifier of the role to retrieve.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, containing a <see cref="RoleServiceResult"/> 
        ///     with the requested role details if found, or an error result if not found or invalid.
        /// </returns>
        Task<RoleServiceResult> GetRoleAsync(string roleId);

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
        ///     Asynchronously removes an assigned role from a user identified by their ID.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to whom the role will be removed.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a result object indicating the outcome of the role removal.
        /// </returns>
        Task<ServiceResult> RemoveAssignedRoleAsync(string id);
    }
}
