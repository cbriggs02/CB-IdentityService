using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Cache;
using IdentityServiceApi.Interfaces.CacheKeys;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityServiceApi.Services.Cache
{
    /// <summary>
    ///     Provides cache-clearing operations related to audit log entries using the in-memory cache system.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    /// </remarks>
    public class AuditLogCacheService : IAuditLogCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly IAuditLogCacheKeyService _cacheKeyService;
        private readonly ILogger<AuditLogCacheService> _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuditLogCacheService"/> class with 
        ///     the specified memory cache and logger.
        /// </summary>
        /// <param name="cache">
        ///     The in-memory cache used to store audit log entries.
        /// </param>
        /// <param name="cacheKeyService">
        ///     The service responsible for generating and managing audit log cache keys.
        /// </param>
        /// <param name="logger">
        ///     The logger instance used to log cache operation events or errors.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if either <paramref name="cache"/> or <paramref name="logger"/> is null.
        /// </exception>
        public AuditLogCacheService(IMemoryCache cache, IAuditLogCacheKeyService cacheKeyService, ILogger<AuditLogCacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _cacheKeyService = cacheKeyService ?? throw new ArgumentNullException(nameof(cacheKeyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        ///     Clears all cached entries related to the audit log list to ensure the 
        ///     cache reflects the most recent data.
        /// </summary>
        public void ClearLogListCache()
        {
            try
            {
                var keys = _cacheKeyService.GetAllLogListKeys();
                foreach (var key in keys)
                {
                    _cache.Remove(key);
                }

                _cacheKeyService.ClearTrackedKeys();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, ErrorMessages.AuditLogCache.FailedToClearLogListCache);
            }
        }
    }
}
