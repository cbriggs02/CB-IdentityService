namespace IdentityServiceApi.Enums
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2026
    ///     @Updated: 2026
    /// </remarks>
    public enum LogSource
    {
        /// <summary>
        /// 
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 
        /// </summary>
        GlobalExceptionMiddleware,

        /// <summary>
        /// 
        /// </summary>
        PermissionService,

        /// <summary>
        /// 
        /// </summary>
        UserService,

        /// <summary>
        /// 
        /// </summary>
        PasswordService,

        /// <summary>
        /// 
        /// </summary>
        PasswordHistoryCleanupService,

        /// <summary>
        /// 
        /// </summary>
        RoleService,

        /// <summary>
        /// 
        /// </summary>
        LoginService,
    }
}
