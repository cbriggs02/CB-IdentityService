using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Context;
using Microsoft.AspNetCore.Identity;

namespace IdentityServiceApi.Features.Authorization.Services
{
    /// <summary>
    ///    Provides authorization services for validating user permissions based on roles and user context.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class AuthorizationService(UserManager<User> userManager, IUserContextService userContextService, IUserLookupService userLookupService) : IAuthorizationService
    {
        /// <summary>
        ///     Validates whether the current authenticated user has permission to access or 
        ///     operate on a resource identified by the specified ID.
        ///     The permission logic is based on the user's roles and identity:
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 SuperAdmin users have unrestricted access.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 Admin users require additional validation via <c>ValidateAdminPermission</c>.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 Non-admin users are only permitted to access their own resources (self-access).
        ///             </description>
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="id">
        ///     The identifier of the target resource or user being accessed. Must not be null or empty.
        /// </param>
        /// <returns>
        ///     A <see cref="Task{Boolean}"/> that resolves to:
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 <c>true</c> if the current user has permission.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 <c>false</c> if the user is not authenticated, lacks required roles, or 
        ///                 does not have sufficient permissions.
        ///             </description>
        ///         </item>
        ///     </returns>
        public async Task<bool> ValidatePermissionAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            var principal = userContextService.GetClaimsPrincipal();
            if (principal == null)
            {
                return false;
            }

            var currentUserId = userContextService.GetUserId(principal);
            if (currentUserId == null)
            {
                return false;
            }

            var roles = userContextService.GetRoles(principal);
            if (roles == null || roles.Count == 0)
            {
                return false;
            }

            if (roles.Contains(Roles.SuperAdmin))
            {
                return true;
            }

            if (roles.Contains(Roles.Admin))
            {
                return await ValidateAdminPermission(id, currentUserId);
            }

            return IsSelfAccess(id, currentUserId);
        }

        private async Task<bool> ValidateAdminPermission(string id, string currentUserId)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            var userLookupResult = await userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return false;
            }

            var user = userLookupResult.UserFound;
            var targetUserRoles = await userManager.GetRolesAsync(user);

            if (targetUserRoles.Contains(Roles.SuperAdmin))
            {
                return false;
            }

            if (targetUserRoles.Contains(Roles.Admin) && !IsSelfAccess(id, currentUserId))
            {
                return false;
            }

            return true;
        }

        private static bool IsSelfAccess(string userId, string currentUserId) =>
            userId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase);
    }
}