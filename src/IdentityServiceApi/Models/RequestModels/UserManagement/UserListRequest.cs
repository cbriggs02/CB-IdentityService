using System.ComponentModel.DataAnnotations;

namespace IdentityServiceApi.Models.RequestModels.UserManagement
{
    /// <summary>
    ///     Represents the model used when requesting a paginated list of users.
    ///     It contains parameters to specify the page number, page size, and optional account status for filtering.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class UserListRequest
    {
        /// <summary>
        ///     Gets or sets the page number of the user list being requested.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0.")]
        public int Page { get; set; }

        /// <summary>
        ///     Gets or sets the size of each page of the user list being requested.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page size must be greater than 0.")]
        public int PageSize { get; set; }

        /// <summary>
        ///     Gets or sets the account status for filtering users.
        ///     1 for active users, 0 for deactivated users.
        /// </summary>
        public int? AccountStatus { get; set; }
    }
}
