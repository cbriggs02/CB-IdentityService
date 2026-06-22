using IdentityServiceApi.Features.UserManagement.Models.DTOs;
using IdentityServiceApi.Shared.Results;

namespace IdentityServiceApi.Features.UserManagement.Models.Results
{
    /// <summary>
    ///     Represents the result of a user-related operation 
    ///     performed by the user service, including a user DTO.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class UserResult : Result
    {
        /// <summary>
        ///     The user DTO object obtained from the operation.
        ///     This may be empty if no users match the request criteria.
        /// </summary>
        public UserDTO User { get; set; } = new UserDTO();
    }
}
