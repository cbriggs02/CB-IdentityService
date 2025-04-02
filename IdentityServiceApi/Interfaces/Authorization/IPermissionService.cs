using IdentityServiceApi.Models.ServiceResultModels.Shared;

namespace IdentityServiceApi.Interfaces.Authorization
{
    /// <summary>
    ///     Interface defining the contract for a service responsible for permission-related operations.
    ///     This service acts as a centralized validator, encapsulating interactions between various services
    ///     and the authorization functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public interface IPermissionService
    {
        /// <summary>
        ///     Asynchronously validates user permissions for a specified user ID.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user whose permissions are being validated.
        /// </param>
        /// <returns>
        ///     A service method that returns a ServiceResult,
        ///     with success status and errors (if any).
        /// </returns>
        Task<ServiceResult> ValidatePermissionsAsync(string id);
    }
}
