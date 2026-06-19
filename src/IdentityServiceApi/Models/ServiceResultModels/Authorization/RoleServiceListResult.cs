using IdentityServiceApi.Models.DTO;

namespace IdentityServiceApi.Models.ServiceResultModels.Authorization
{
    /// <summary>
    ///     Represents the result of a service operation that retrieves a list of roles.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class RoleServiceListResult
    {
        /// <summary>
        ///     Gets or sets the collection of roles returned from the service operation.
        /// </summary>
        public IEnumerable<RoleDTO> Roles { get; set; } = new List<RoleDTO>();
    }
}
