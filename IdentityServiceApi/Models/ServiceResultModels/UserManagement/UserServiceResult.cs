using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.ServiceResultModels.Shared;

namespace IdentityServiceApi.Models.ServiceResultModels.UserManagement
{
    /// <summary>
    ///     Represents the result of a user-related operation 
    ///     performed by the user service, including a user DTO.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class UserServiceResult : ServiceResult
    {
        /// <summary>
        ///     The user DTO object obtained from the operation.
        ///     This may be empty if no users match the request criteria.
        /// </summary>
        public UserDTO User { get; set; } = new UserDTO();
    }
}
