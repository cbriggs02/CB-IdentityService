using IdentityServiceApi.Models.Entities;

namespace IdentityServiceApi.Interfaces.CacheKeys
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025
    /// </remarks>
    public interface IAuditLogCacheKeyService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        string GetAuditLogListKey(int page, int pageSize, AuditAction? action);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllLogListKeys();

        /// <summary>
        /// 
        /// </summary>
        void ClearTrackedKeys();
    }
}
