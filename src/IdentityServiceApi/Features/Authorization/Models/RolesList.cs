namespace IdentityServiceApi.Features.Authorization.Models
{
    /// <summary>
    ///     Represents the response returned by the roles API when retrieving roles.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class RolesListResponse
    {
        /// <summary>
        ///     The list of roles retrieved from the service that is returned by the API.
        ///     This collection may be empty if no roles are available.
        /// </summary>
        public IEnumerable<RoleDTO> Roles { get; set; } = [];
    }
}
