namespace IdentityServiceApi.Helpers.CacheKeys
{
    /// <summary>
    ///     Contains cache key constants related to role data.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025  
    /// </remarks>
    public static class RolesCacheKeys
    {
        /// <summary>
        ///     The domain name used as a prefix for all role-related cache keys.
        /// </summary>
        public const string DomainName = "role";

        /// <summary>
        ///     The cache key for storing the list of all roles.
        /// </summary>
        public const string RoleList = $"{DomainName}:list";
    }
}
