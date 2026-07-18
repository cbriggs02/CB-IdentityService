namespace IdentityServiceApi.Features.Authorization.Models
{
    /// <summary>
    ///     Represents the result of a service operation that retrieves a list of roles.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class RoleListResult
    {
        /// <summary>
        ///     Gets or sets the collection of roles returned from the service operation.
        /// </summary>
        public IEnumerable<RoleDTO> Roles { get; set; } = [];
    }
}
