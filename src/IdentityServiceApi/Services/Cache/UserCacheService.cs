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
    ///     @Updated: 2026
    /// </remarks>
    public class UserCacheService(IMemoryCache cache, IUserCacheKeyService cacheKeyService, ILogger<UserCacheService> logger) : IUserCacheService
    {
        private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        private readonly IUserCacheKeyService _cacheKeyService = cacheKeyService ?? throw new ArgumentNullException(nameof(cacheKeyService));
        private readonly ILogger<UserCacheService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
