namespace IdentityServiceApi.Interfaces.CacheKeys
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    /// </remarks>
    public interface IUserCacheKeyService
    {
        /// <summary>
        /// 
        /// </summary>
        string CreationStatsKey { get; }

        /// <summary>
        /// 
        /// </summary>
        string StateMetricsKey { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="accountStatus"></param>
        /// <returns></returns>
        string GetUserListKey(int page, int pageSize, int? accountStatus);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllUserListKeys();

        /// <summary>
        /// 
        /// </summary>
        void ClearTrackedKeys();
    }
}
