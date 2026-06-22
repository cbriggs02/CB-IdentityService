using IdentityServiceApi.Features.Authentication.Interfaces;
using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Shared.Constants;
using Microsoft.AspNetCore.Identity;

namespace IdentityServiceApi.Features.Authorization.Services
{
    /// <summary>
    ///     Service responsible for interacting with authorization-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class AuthorizationService(UserManager<User> userManager, IUserContextService userContextService, IUserLookupService userLookupService) : IAuthorizationService
    {
        /// <summary>
        ///     Asynchronously validates permissions based on the current user's role and the target user's data:
        ///     - Regular users can only access their own data.
        ///     - Admin users can access their own data and any non-admin user's data,
        ///       but are restricted from accessing data of other admin users.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user whose permissions are being validated. 
        ///     This represents the target user whose data the current user is attempting to access.
        /// </param>
        /// <returns>
        ///     True if the current user has permission to access the target user's data; otherwise, false.
        ///     - Returns true if the current user is a regular user accessing their own data.
        ///     - Returns true if the current user is an admin accessing their own data or non-admin data.
        ///     - Returns false if an admin attempts to access another admin's data.
        ///     -Returns false if id is not provided or user context is not available.
        /// </returns>
        public async Task<bool> ValidatePermissionAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false; // No ID provided, so permission cannot be validated
            }

            var principal = userContextService.GetClaimsPrincipal();
            if (principal == null)
            {
                return false; // No user context available, so permission cannot be validated
            }

            var currentUserId = userContextService.GetUserId(principal);
            if (currentUserId == null)
            {
                return false; // No id recovered from http context, deny by default
            }

            var roles = userContextService.GetRoles(principal);
            if (roles == null || roles.Count == 0)
            {
                return false; // No roles assigned, deny access
            }

            if (roles.Contains(Roles.Admin))
            {
                return await ValidateAdminPermission(id, currentUserId);
            }

            if (roles.Contains(Roles.SuperAdmin))
            {
                return true; // super admin can access any endpoint and data.
            }

            return IsSelfAccess(id, currentUserId);
        }

        private async Task<bool> ValidateAdminPermission(string id, string currentUserId)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false; // No target user ID provided, so permission cannot be validated
            }

            var userLookupResult = await userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return false;
            }

            var user = userLookupResult.UserFound;

            if (await IsTargetSuperAdmin(user))
            {
                return false; // Can't access Super Admin
            }


            if (await IsTargetAdmin(user) && !IsSelfAccess(id, currentUserId))
            {
                return false; // Can't access another admin's data or super admin data
            }

            return true;
        }

        private static bool IsSelfAccess(string userId, string currentUserId) =>
            userId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase);

        private async Task<bool> IsTargetAdmin(User user)
        {
            var targetUserRoles = await userManager.GetRolesAsync(user);
            return targetUserRoles.Any(role => role == Roles.Admin);
        }

        private async Task<bool> IsTargetSuperAdmin(User user)
        {
            var targetUserRoles = await userManager.GetRolesAsync(user);
            return targetUserRoles.Any(role => role == Roles.SuperAdmin);
        }
    }
}
