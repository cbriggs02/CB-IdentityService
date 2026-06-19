using IdentityServiceApi.Models.DTO;

namespace IdentityServiceApi.Models.ApiResponseModels.Users
{
    /// <summary>
    ///     Represents the API response for user creation statistics, containing a list of user creation metrics.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public class UserCreationStatsResponse
    {
        /// <summary>
        ///     Gets or sets the list of user creation statistics, each containing a date and the count of users created on that date.
        /// </summary>
        public List<UserCreationStatDTO> UserCreationStats { get; set; } = new List<UserCreationStatDTO>();
    }
}
