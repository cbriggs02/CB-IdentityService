using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Authentication;
using IdentityServiceApi.Interfaces.Authorization;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentityServiceApi.Services.Authorization
{
    /// <summary>
    ///     Service responsible for interacting with authorization-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserContextService _userContextService;
        private readonly IUserLookupService _userLookupService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthorizationService"/> class.
        /// </summary>
        /// <param name="userManager">
        ///     The user manager responsible for handling user management operations.
        ///     In this case used for locating users during permission validation.
        /// </param>
        /// <param name="userContextService">
        ///     The service used for accessing current HTTP context.
        /// </param>
        /// <param name="userLookupService">'
        ///     The service used for looking up users in the system.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public AuthorizationService(UserManager<User> userManager, IUserContextService userContextService, IUserLookupService userLookupService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
            _userLookupService = userLookupService ?? throw new ArgumentNullException(nameof(userLookupService));
        }

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
        public async Task<bool> ValidatePermission(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false; // No ID provided, so permission cannot be validated
            }

            var principal = _userContextService.GetClaimsPrincipal();
            if (principal == null)
            {
                return false; // No user context available, so permission cannot be validated
            }

            var currentUserId = _userContextService.GetUserId(principal);
            var roles = _userContextService.GetRoles(principal);

            if (roles == null || !roles.Any())
            {
                return false; // No roles assigned, deny access
            }

            if (roles.Contains(Roles.Admin))
            {
                return await ValidateAdminPermission(id, currentUserId);
            }

            if(roles.Contains(Roles.SuperAdmin)) 
            {
                return true; // super admin can access any endpoint and data.
            }

            return IsSelfAccess(id, currentUserId);
        }

        /// <summary>
        ///     Asynchronously validates whether an admin user has permission to perform actions on another user.
        ///     Admin users can access any user except other admins.
        /// </summary>
        /// <param name="id">
        ///     The ID of the target user.
        /// </param>
        /// <param name="currentUserId">
        ///     The ID of the current admin user.
        /// </param>
        /// <returns>
        ///     True if the current user has permission;
        ///     False if if the target user's is is null;
        ///     False if no user found matching target id;
        ///     False is target user is a super admin;
        ///     False is target user is admin or super;
        /// </returns>
        private async Task<bool> ValidateAdminPermission(string id, string currentUserId)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false; // No target user ID provided, so permission cannot be validated
            }

            var userLookupResult = await _userLookupService.FindUserById(id);
            if(!userLookupResult.Success)
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

        private static bool IsSelfAccess(string userId, string currentUserId)
        {
            return userId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase);
        }

        private async Task<bool> IsTargetAdmin(User user)
        {
            var targetUserRoles = await _userManager.GetRolesAsync(user);
            return targetUserRoles.Any(role => role == Roles.Admin);
        }

        private async Task<bool> IsTargetSuperAdmin(User user)
        {
            var targetUserRoles = await _userManager.GetRolesAsync(user);
            return targetUserRoles.Any(role => role == Roles.SuperAdmin);
        }
    }
}
