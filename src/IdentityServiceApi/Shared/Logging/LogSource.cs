namespace IdentityServiceApi.Shared.Logging
{
    /// <summary>
    ///     Defines the source or originating component of a log entry.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2026  
    ///     @Updated: 2026  
    /// </remarks>
    public enum LogSource
    {
        /// <summary>
        ///     Represents an unknown or unspecified log source.
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     Indicates the log originated from the global exception handling middleware.
        /// </summary>
        GlobalExceptionMiddleware,

        /// <summary>
        ///     Indicates the log originated from the permission service.
        /// </summary>
        PermissionService,

        /// <summary>
        ///     Indicates the log originated from the user management service.
        /// </summary>
        UserService,

        /// <summary>
        ///     Indicates the log originated from the password service.
        /// </summary>
        PasswordService,

        /// <summary>
        ///     Indicates the log originated from the password history cleanup service.
        /// </summary>
        PasswordHistoryCleanupService,

        /// <summary>
        ///     Indicates the log originated from the role management service.
        /// </summary>
        RoleService,

        /// <summary>
        ///     Indicates the log originated from the login/authentication service.
        /// </summary>
        LoginService,
    }
}