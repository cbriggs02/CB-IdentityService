namespace IdentityServiceApi.Constants
{
    /// <summary>
    ///     Contains grouped role combinations used for role-based authorization in the application.
    ///     These constants allow for reusable and centralized management of common role sets across the API.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public static class RoleGroups
    {
        /// <summary>
        ///     Represents all standard roles in the system, including SuperAdmin, Admin, and User.
        ///     This group can be used to authorize endpoints that require access by any standard user type.
        /// </summary>
        public const string AllStandardRoles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.User}";

        /// <summary>
        ///     Represents only administrative roles in the system, including SuperAdmin and Admin.
        ///     This group is used to restrict access to endpoints that require elevated privileges.
        /// </summary>
        public const string AdminOnly = $"{Roles.SuperAdmin},{Roles.Admin}";
    }
}
