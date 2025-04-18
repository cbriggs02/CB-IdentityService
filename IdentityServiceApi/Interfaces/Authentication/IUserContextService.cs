using System.Net;
using System.Security.Claims;

namespace IdentityServiceApi.Interfaces.Authentication
{
    /// <summary>
    ///     Defines methods for retrieving user context information, such as claims, user ID,
    ///     IP address, request path, and user roles. This service is used to abstract 
    ///     the access to user-related data from the HTTP context.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public interface IUserContextService
    {
        /// <summary>
        ///     Retrieves the claims principal of the currently authenticated user.
        ///     This includes all claims associated with the user, such as identity
        ///     and roles, allowing for easy access to user information.
        /// </summary>
        /// <returns>
        ///     The claims principal representing the current user.
        /// </returns>
        ClaimsPrincipal? GetClaimsPrincipal();

        /// <summary>
        ///     Extracts the ID of the currently authenticated user from the provided 
        ///     claims principal. This ID is typically used for identifying the user
        ///     in various operations throughout the application.
        /// </summary>
        /// <param name="principal">
        ///     The claims principal representing the currently authenticated user.
        /// </param>
        /// <returns>
        ///     The user's ID as a string, or null if not found in the claims.
        /// </returns>
        string? GetUserId(ClaimsPrincipal principal);

        /// <summary>
        ///     Retrieves a list of roles associated with the currently authenticated user
        ///     from the provided claims principal. This allows for role-based access 
        ///     control and permission checks within the application.
        /// </summary>
        /// <param name="principal">
        ///     The claims principal representing the currently authenticated user.
        /// </param>
        /// <returns>
        ///     A list of role names associated with the user, or an empty list if none.
        /// </returns>
        List<string> GetRoles(ClaimsPrincipal principal);

        /// <summary>
        ///     Retrieves the remote IP address of the client making the current
        ///     HTTP request. This information can be useful for logging,
        ///     security, and auditing purposes.
        /// </summary>
        /// <returns>
        ///     The remote IP address of the client as an <see cref="IPAddress"/>.
        /// </returns>
        IPAddress? GetAddress();

        /// <summary>
        ///     Retrieves the request path from the current HTTP context. This 
        ///     information is essential for logging and auditing the actions
        ///     performed by the user.
        /// </summary>
        /// <returns>
        ///     The request path as a string, or an empty string if not available.
        /// </returns>
        string? GetRequestPath();
    }
}
