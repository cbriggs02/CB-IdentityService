namespace IdentityServiceApi.Interfaces.Cache
{
    /// <summary>
    ///     Defines cache management operations specific to audit logs.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    /// </remarks>
    public interface IAuditLogCacheService
    {
        /// <summary>
        ///     Clears the cached list of audit logs from memory, typically called after log changes
        ///     such as additions or deletions to ensure cache consistency.
        /// </summary>
        void ClearLogListCache();
    }
}
