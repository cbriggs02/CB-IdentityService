using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.ServiceResultModels.UserManagement;

namespace IdentityServiceApi.Interfaces.Utilities
{
    /// <summary>
    ///     Interface for creating service results related to user lookups.
    ///     This interface defines methods for creating both success and failure results 
    ///     for user lookup operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public interface IUserLookupServiceResultFactory : IServiceResultFactory
    {
        /// <summary>
        ///     Creates a failed user lookup service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="UserLookupServiceResult"/> indicating failure along with the provided errors.
        /// </returns>
        UserLookupServiceResult UserLookupOperationFailure(string[] errors);

        /// <summary>
        ///     Creates a successful user lookup service result with the specified user.
        /// </summary>
        /// <param name="user">
        ///     The <see cref="User"/> object representing the successfully looked up user.
        /// </param>
        /// <returns>
        ///     A <see cref="UserLookupServiceResult"/> containing the success status and the user data.
        /// </returns>
        UserLookupServiceResult UserLookupOperationSuccess(User user);
    }
}
