namespace IdentityServiceApi.Interfaces.Authorization
{
    /// <summary>
    ///     Interface defining the contract for a service responsible for authorization-related operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public interface IAuthorizationService
    {
        /// <summary>
        ///     Asynchronously validates permissions to ensure that regular users can only access their own data,
        ///     and that admin users can access their own data or non-admin users' data, 
        ///     but are restricted from accessing other admins' data.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user whose permissions are being validated. This represents the target user 
        ///     whose data the current user is attempting to access.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns a boolean value:
        ///     - true if the current user has valid permissions to access the target user's data.
        ///     - false if the current user lacks permissions to access the target user's data.
        ///     Regular users are restricted to accessing their own data, 
        ///     while admins can access their own and other non-admin users' data.
        /// </returns>
        Task<bool> ValidatePermissionAsync(string id);
    }
}
