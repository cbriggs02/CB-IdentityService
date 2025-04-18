using IdentityServiceApi.Interfaces.Authentication;
using System.Net;
using System.Security.Claims;

namespace IdentityServiceApi.Services.Authentication
{
    /// <summary>
    ///     Provides methods for accessing user context information from the HTTP context,
    ///     including claims, user ID, IP address, request path, and user roles. This service
    ///     abstracts the retrieval of user-related data to facilitate logging and authorization 
    ///     processes within the application.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserContextService"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">
        ///     The HTTP context accessor used to retrieve the current HTTP context.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the <paramref name="httpContextAccessor"/> is null.
        /// </exception>
        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        ///     Retrieves the claims principal of the currently authenticated user.
        ///     This includes all claims associated with the user, which can be used for
        ///     authorization checks and logging.
        /// </summary>
        /// <returns>
        ///     The claims principal representing the current user, or null if the user is not authenticated.
        /// </returns>
        public ClaimsPrincipal? GetClaimsPrincipal()
        {
            return _httpContextAccessor.HttpContext?.User;
        }

        /// <summary>
        ///     Extracts the ID of the currently authenticated user from the provided claims principal.
        ///     This ID is typically used for identifying the user in various operations throughout the application.
        /// </summary>
        /// <param name="principal">
        ///     The claims principal representing the currently authenticated user.
        /// </param>
        /// <returns>
        ///     The user's ID as a string, or null if not found in the claims.
        /// </returns>
        public string? GetUserId(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                return null;
            }
            return principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <summary>
        ///     Retrieves a list of roles associated with the currently authenticated user
        ///     from the provided claims principal. This allows for role-based access control 
        ///     and permission checks within the application.
        /// </summary>
        /// <param name="principal">
        ///     The claims principal representing the currently authenticated user.
        /// </param>
        /// <returns>
        ///     A list of role names associated with the user, or an empty list if none.
        /// </returns>
        public List<string> GetRoles(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                return new List<string>();
            }
            return principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        }

        /// <summary>
        ///     Retrieves the remote IP address of the client making the current HTTP request.
        ///     This information is useful for logging, security, and auditing purposes.
        /// </summary>
        /// <returns>
        ///     The remote IP address of the client as an <see cref="IPAddress"/>.
        /// </returns>
        public IPAddress? GetAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress;
        }

        /// <summary>
        ///     Retrieves the request path from the current HTTP context. This information is 
        ///     essential for logging and auditing the actions performed by the user.
        /// </summary>
        /// <returns>
        ///     The request path as a string, or an empty string if not available.
        /// </returns>
        public string? GetRequestPath()
        {
            return _httpContextAccessor.HttpContext?.Request?.Path.Value;
        }
    }
}
