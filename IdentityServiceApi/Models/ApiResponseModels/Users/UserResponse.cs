using IdentityServiceApi.Models.DTO;

namespace IdentityServiceApi.Models.ApiResponseModels.UsersResponses
{
    /// <summary>
    ///     Represents the response returned by the users API when retrieving a user, or creating a user.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class UserResponse
    {
        /// <summary>
        ///     The User DTO object retrieved or created from the service that is returned by the API.
        ///     This will contain user details if successful.
        /// </summary>
        public UserDTO User { get; set; } = new UserDTO();
    }
}
