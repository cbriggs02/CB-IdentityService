using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityServiceApi.Features.UserManagement.Caching
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
    public class UserCacheService(IMemoryCache cache, IUserCacheKeyService cacheKeyService, ILoggerService loggerService) : IUserCacheService
    {
        /// <summary>
        ///     Clears all tracked user list cache entries, including paginated and filtered user list data.
        ///     This ensures that any updates to user data are reflected immediately.
        /// </summary>
        public void ClearUserListCache()
        {
            try
            {
                var keys = cacheKeyService.GetAllUserListKeys();
                foreach (var key in keys)
                {
                    cache.Remove(key);
                }

                cacheKeyService.ClearTrackedKeys();
            }
            catch (Exception ex)
            {
                var logEntry = new LogEntry
                {
                    LogLevel = LogLevel.Error,
                    LogSource = LogSource.UserCacheService,
                    Message = ErrorMessages.UserCache.FailedToClearUserListCache,
                    Exception = ex
                };

                loggerService.LogData(logEntry);
            }
        }
    }
}
