namespace IdentityServiceApi.Interfaces.Cache
{
    /// <summary>
    ///     Defines the contract for managing and clearing user-related cache entries.
    ///     This service is responsible for invalidating specific cached data when user data changes,
    ///     ensuring the cache remains consistent with the current state of the application.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    ///     @Updated: 2026
    /// </remarks>
    public interface IUserCacheService
    {
        /// <summary>
        ///     Clears all tracked cache entries for paginated user lists.
        ///     This is important after user data modifications to ensure the UI reflects updated information.
        /// </summary>
        void ClearUserListCache();
    }
}
