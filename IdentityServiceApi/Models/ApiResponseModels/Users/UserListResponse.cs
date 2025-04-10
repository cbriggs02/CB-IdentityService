using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.Shared;

namespace IdentityServiceApi.Models.ApiResponseModels.UsersResponses
{
    /// <summary>
    ///     Represents the response returned by the users API when retrieving users with pagination.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class UserListResponse
    {
        /// <summary>
        ///     The list of users retrieved from the service.
        /// </summary>
        public IEnumerable<UserDTO> Users { get; set; } = new List<UserDTO>();

        /// <summary>
        ///     Metadata for pagination, such as total count and page details.
        /// </summary>
        public PaginationModel PaginationMetadata { get; set; } = new PaginationModel();
    }
}
