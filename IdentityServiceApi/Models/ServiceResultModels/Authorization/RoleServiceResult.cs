using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.ServiceResultModels.Shared;

namespace IdentityServiceApi.Models.ServiceResultModels.Authorization
{
    /// <summary>
    ///     Represents the result of a role-related service operation.
    ///     Inherits from <see cref="ServiceResult"/> to indicate the success or failure of the operation,
    ///     and includes the corresponding <see cref="RoleDTO"/> data if the operation was successful.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    /// </remarks>
    public class RoleServiceResult : ServiceResult
    {
        /// <summary>
        ///     Gets or sets the <see cref="RoleDTO"/> that contains detailed role information
        ///     returned as part of the service operation result.
        /// </summary>
        public RoleDTO Role { get; set; } = new RoleDTO();
    }
}
