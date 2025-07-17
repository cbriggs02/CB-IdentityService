using IdentityServiceApi.Interfaces.CacheKeys;

namespace IdentityServiceApi.Services.CacheKeys
{
    /// <summary>
    ///     Provides methods for generating and managing user-related cache keys.
    ///     Used to uniquely identify user list data and related user statistics in the cache.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    /// </remarks>
    public class UserCacheKeyService : IUserCacheKeyService
    {
        private const string DomainName = "user";
        private readonly HashSet<string> _trackedUserListKeys = new();

        /// <summary>
        ///     Gets the cache key used for storing user creation statistics.
        /// </summary>
        public string CreationStatsKey => $"{DomainName}:creation-stats";

        /// <summary>
        ///     Gets the cache key used for storing user account state metrics.
        /// </summary>
        public string StateMetricsKey => $"{DomainName}:state-metrics";

        /// <summary>
        ///     Generates a unique cache key for a paginated user list based on page number, page size, and account status.
        /// </summary>
        /// <param name="page">
        ///     The page number of the user list.
        /// </param>
        /// <param name="pageSize">
        ///     The number of users per page.
        /// </param>
        /// <param name="accountStatus">
        ///     The account status filter applied to the user list; can be null if no status filter is applied.
        /// </param>
        /// <returns>
        ///     A unique cache key string representing the specific user list query.
        /// </returns>
        public string GetUserListKey(int page, int pageSize, int? accountStatus)
        {
            var key = $"{DomainName}:list:Page:{page}:size:{pageSize}:status:{accountStatus?.ToString() ?? "null"}";

            lock (_trackedUserListKeys)
            {
                _trackedUserListKeys.Add(key);
            }

            return key;
        }

        /// <summary>
        ///     Retrieves all tracked user list cache keys generated during the application's lifetime.
        /// </summary>
        /// <returns>
        ///     A collection of all user list cache keys that have been generated and tracked.
        /// </returns>
        public IEnumerable<string> GetAllUserListKeys()
        {
            lock (_trackedUserListKeys)
            {
                return _trackedUserListKeys.ToList();
            }
        }

        /// <summary>
        ///     Clears all tracked user list cache keys from the internal collection.
        /// </summary>
        public void ClearTrackedKeys()
        {
            lock (_trackedUserListKeys)
            {
                _trackedUserListKeys.Clear();
            }
        }
    }
}
