using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.ServiceResultModels.Shared;

namespace IdentityServiceApi.Models.ServiceResultModels.UserManagement
{
    /// <summary>
    ///     Represents the result of a user lookup operation.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class UserLookupServiceResult : ServiceResult
    {
        /// <summary>
        ///     Gets or sets the user found during the lookup.
        /// </summary>
        public User UserFound { get; set; } = new User();
    }
}
