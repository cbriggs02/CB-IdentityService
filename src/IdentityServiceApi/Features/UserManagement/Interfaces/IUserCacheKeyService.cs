namespace IdentityServiceApi.Features.UserManagement.Interfaces
{
    /// <summary>
    ///     Provides a central contract for producing and tracking cache keys related to user data.
    ///     Implementations create consistent cache keys for telemetry and paged user-list responses
    ///     and expose the set of tracked list keys so caches can be invalidated or enumerated.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    ///     @Updated: 2026
    /// </remarks>
    public interface IUserCacheKeyService
    {
        /// <summary>
        ///     Constructs a cache key that uniquely identifies a paged list of users.
        ///     The key should incorporate the requested page, page size, and optional
        ///     account status so that different query combinations map to distinct cache entries.
        /// </summary>
        /// <param name="page">
        ///     The page number (zero- or one-based depending on implementation) for the list.
        /// </param>
        /// <param name="pageSize">
        ///     The number of items per page.
        /// </param>
        /// <param name="accountStatus">
        ///     Optional account status filter. When <c>null</c>, the key should represent
        ///     an unfiltered list (all statuses).
        /// </param>
        /// <returns>
        ///     A string cache key that uniquely identifies the requested paged user list.
        ///     Implementations may follow a pattern such as <c>users:list:page={page}:size={pageSize}:status={status}</c>.
        /// </returns>
        string GetUserListKey(int page, int pageSize, int? accountStatus);

        /// <summary>
        ///     Returns all tracked cache keys that represent user list entries.
        ///     This collection is useful when invalidating or iterating over every
        ///     cached paged user list that the service has generated.
        /// </summary>
        /// <returns>
        ///     An enumerable of cache key strings for user list entries.
        /// </returns>
        IEnumerable<string> GetAllUserListKeys();

        /// <summary>
        ///     Clears any internal tracking of generated cache keys.
        ///     After calling this method, <see cref="GetAllUserListKeys"/> should return
        ///     an empty collection until new keys are generated.
        /// </summary>
        void ClearTrackedKeys();
    }
}