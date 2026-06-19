namespace IdentityServiceApi.Constants
{
    /// <summary>
    ///     Contains constants for role names used throughout the application.
    ///     These constants represent the various user roles that are available in the system,
    ///     such as super administrative, administrative and regular user roles.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public static class Roles
    {
        /// <summary>
        ///     Represents the super administrative role.
        ///     Users with this role have the highest level of access and control in the system.
        /// </summary>
        public const string SuperAdmin = "SuperAdmin";

        /// <summary>
        ///     Represents the administrative role.
        ///     Users with this role have elevated privileges to manage certain system operations.
        /// </summary>
        public const string Admin = "Admin";

        /// <summary>
        ///     Represents the regular user role.
        ///     Users with this role have standard access to the application's features and functionality.
        /// </summary>
        public const string User = "User";
    }
}
