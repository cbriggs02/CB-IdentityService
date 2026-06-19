using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.Shared;

namespace IdentityServiceApi.Models.ServiceResultModels.UserManagement
{
    /// <summary>
    ///     Represents the result of a user-related operation 
    ///     performed by the user service, including a list of users 
    ///     and pagination metadata.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class UserServiceListResult
    {
        /// <summary>
        ///     Contains users retrieved from the operation.
        ///     This list may be empty if no users match the request criteria.
        /// </summary>  
        public IEnumerable<SimplifiedUserDTO> Users { get; set; } = new List<SimplifiedUserDTO>();

        /// <summary>
        ///     Gets or sets the pagination metadata associated with the user retrieval operation.
        ///     This metadata includes details such as total user count, current page, and page size. 
        /// </summary>
        public PaginationModel PaginationMetadata { get; set; } = new PaginationModel();
    }
}
