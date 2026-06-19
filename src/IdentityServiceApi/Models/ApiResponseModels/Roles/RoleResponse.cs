using IdentityServiceApi.Models.DTO;

namespace IdentityServiceApi.Models.ApiResponseModels.Roles
{
    /// <summary>
    ///     Represents the API response model returned when retrieving information about a specific role.
    ///     This model contains a single <see cref="RoleDTO"/> instance representing the role's data.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    /// </remarks>
    public class RoleResponse
    {
        /// <summary>
        ///     Gets or sets the role data transfer object containing detailed information
        ///     about the retrieved role.
        /// </summary>
        public RoleDTO Role { get; set; } = new RoleDTO();
    }
}
