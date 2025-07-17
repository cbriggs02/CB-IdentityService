using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Cache;
using IdentityServiceApi.Interfaces.CacheKeys;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityServiceApi.Services.Cache
{
    /// <summary>
    ///     Provides an implementation of <see cref="IUserCacheService"/> that handles
    ///     clearing specific user-related cache entries. This service ensures that 
    ///     outdated or stale data is removed from the cache to maintain consistency 
    ///     and reliability across user-related operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    /// </remarks>
    public class UserCacheService : IUserCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly IUserCacheKeyService _cacheKeyService;
        private readonly ILogger<UserCacheService> _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserCacheService"/> class.
        /// </summary>
        /// <param name="cache">
        ///     The memory cache used for storing and removing user-related cache entries.
        /// </param>
        /// <param name="cacheKeyService">
        ///     The service responsible for generating and managing user-related cache keys.
        /// </param>
        /// <param name="logger">
        ///     The logger used for logging warning messages when cache clearing operations fail.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if either <paramref name="cache"/> or <paramref name="logger"/> is null.
        /// </exception>
        public UserCacheService(IMemoryCache cache, IUserCacheKeyService cacheKeyService, ILogger<UserCacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _cacheKeyService = cacheKeyService ?? throw new ArgumentNullException(nameof(cacheKeyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        ///     Clears the cache entry related to user creation statistics.
        ///     This should be invoked whenever a new user is created or related metrics need to be refreshed.
        /// </summary>
        public void ClearCreationStatsCache()
        {
            try
            {
                _cache.Remove(_cacheKeyService.CreationStatsKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, ErrorMessages.UserCache.FailedToClearCreationStatsCache);
            }
        }

        /// <summary>
        ///     Clears the cache entry for user state metrics.
        ///     This is typically used after updates to user account statuses.
        /// </summary>
        public void ClearStateMetricsCache()
        {
            try
            {
                _cache.Remove(_cacheKeyService.StateMetricsKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, ErrorMessages.UserCache.FailedToClearStateMetricsCache);
            }
        }

        /// <summary>
        ///     Clears all tracked user list cache entries, including paginated and filtered user list data.
        ///     This ensures that any updates to user data are reflected immediately.
        /// </summary>
        public void ClearUserListCache()
        {
            try
            {
                var keys = _cacheKeyService.GetAllUserListKeys();
                foreach (var key in keys)
                {
                    _cache.Remove(key);
                }

                _cacheKeyService.ClearTrackedKeys();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, ErrorMessages.UserCache.FailedToClearUserListCache);
            }
        }
    }
}
