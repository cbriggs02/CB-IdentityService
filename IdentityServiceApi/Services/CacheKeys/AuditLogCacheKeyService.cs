using IdentityServiceApi.Interfaces.CacheKeys;
using IdentityServiceApi.Models.Entities;

namespace IdentityServiceApi.Services.CacheKeys
{
    /// <summary>
    ///     Provides methods for generating and managing audit log-related cache keys.
    ///     Used to uniquely identify cached audit log list queries.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    /// </remarks>
    public class AuditLogCacheKeyService : IAuditLogCacheKeyService
    {
        /// <summary>
        ///     The domain prefix used for audit log cache keys.
        /// </summary>
        public const string DomainName = "audit-log";

        private static readonly HashSet<string> _trackedLogListKeys = new();

        /// <summary>
        ///     Generates a unique cache key for a paginated audit log list based on page number, 
        ///     page size, and optional action filter.
        /// </summary>
        /// <param name="page">
        ///     The page number of the audit log list.
        /// </param>
        /// <param name="pageSize">
        ///     The number of audit log entries per page.
        /// </param>
        /// <param name="action">
        ///     An optional audit action filter; can be null if no action filter is applied.
        /// </param>
        /// <returns>
        ///     A unique cache key string representing the specific audit log list query.
        /// </returns>
        public string GetAuditLogListKey(int page, int pageSize, AuditAction? action)
        {
            var key = $"{DomainName}:list:page:{page}:size:{pageSize}:action:{action?.ToString() ?? "null"}";

            lock (_trackedLogListKeys)
            {
                _trackedLogListKeys.Add(key);
            }

            return key;
        }

        /// <summary>
        ///     Retrieves all tracked audit log list cache keys generated during the application's lifetime.
        /// </summary>
        /// <returns>
        ///     A collection of all audit log list cache keys that have been generated and tracked.
        /// </returns>
        public IEnumerable<string> GetAllLogListKeys()
        {
            lock (_trackedLogListKeys)
            {
                return _trackedLogListKeys.ToList();
            }
        }

        /// <summary>
        ///     Clears all tracked audit log list cache keys from the internal collection.
        /// </summary>
        public void ClearTrackedKeys()
        {
            lock (_trackedLogListKeys)
            {
                _trackedLogListKeys.Clear();
            }
        }
    }
}
