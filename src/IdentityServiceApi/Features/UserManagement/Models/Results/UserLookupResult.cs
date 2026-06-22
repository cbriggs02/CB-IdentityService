using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Shared.Results;

namespace IdentityServiceApi.Features.UserManagement.Models.Results
{
    /// <summary>
    ///     Represents the result of a user lookup operation.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class UserLookupResult : Result
    {
        /// <summary>
        ///     Gets or sets the user found during the lookup.
        /// </summary>
        public User UserFound { get; set; } = new User();
    }
}
