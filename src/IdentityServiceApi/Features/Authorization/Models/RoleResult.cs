using IdentityServiceApi.Shared.ResultFactories;

namespace IdentityServiceApi.Features.Authorization.Models
{
    /// <summary>
    ///     Represents the result of a role-related service operation.
    ///     Inherits from <see cref="Result"/> to indicate the success or failure of the operation,
    ///     and includes the corresponding <see cref="RoleDTO"/> data if the operation was successful.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    ///     @Updated: 2026
    /// </remarks>
    public class RoleResult : Result
    {
        /// <summary>
        ///     Gets or sets the <see cref="RoleDTO"/> that contains detailed role information
        ///     returned as part of the service operation result.
        /// </summary>
        public RoleDTO Role { get; set; } = new RoleDTO();
    }
}
