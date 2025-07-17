using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Services.CacheKeys;

namespace IdentityServiceApi.Tests.Unit.Services.CacheKeys
{
    /// <summary>
    ///     Unit tests for the <see cref="AuditLogCacheKeyService"/> class.
    ///     This class contains test cases for various audit log cache key service scenarios, verifying the 
    ///     behavior of the audit log cache key service functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class AuditLogCacheKeyServiceTests
    {
        private readonly AuditLogCacheKeyService _service = new();

        /// <summary>
        ///     Verifies that the <see cref="AuditLogCacheKeyService.GetAuditLogListKey(int, int, AuditAction?)"/>
        ///     method returns the correct key format when given valid page, pageSize, and audit action input.
        /// </summary>
        [Fact]
        public void GetAuditLogListKey_ValidInput_ReturnsCorrectKey()
        {
            // Arrange
            int page = 1;
            int pageSize = 10;
            AuditAction? action = AuditAction.AuthorizationBreach;

            // Act
            var key = _service.GetAuditLogListKey(page, pageSize, action);

            // Assert
            Assert.Equal("audit-log:list:page:1:size:10:action:AuthorizationBreach", key);
        }

        /// <summary>
        ///     Verifies that the <see cref="AuditLogCacheKeyService.GetAuditLogListKey(int, int, AuditAction?)"/>
        ///     method correctly includes "null" in the cache key when the audit action is not provided.
        /// </summary>
        [Fact]
        public void GetAuditLogListKey_NullAction_IncludesNullInKey()
        {
            // Act
            var key = _service.GetAuditLogListKey(2, 20, null);

            // Assert
            Assert.Equal("audit-log:list:page:2:size:20:action:null", key);
        }

        /// <summary>
        ///     Verifies that the <see cref="AuditLogCacheKeyService.GetAllLogListKeys"/> method
        ///     returns all audit log cache keys that were previously generated and tracked.
        /// </summary>
        [Fact]
        public void GetAllLogListKeys_ReturnsPreviouslyGeneratedKeys()
        {
            // Arrange
            _service.ClearTrackedKeys(); // Ensure clean state
            var key1 = _service.GetAuditLogListKey(1, 10, AuditAction.Exception);
            var key2 = _service.GetAuditLogListKey(2, 20, null);

            // Act
            var keys = _service.GetAllLogListKeys().ToList();

            // Assert
            Assert.Contains(key1, keys);
            Assert.Contains(key2, keys);
            Assert.Equal(2, keys.Count);
        }

        /// <summary>
        ///     Verifies that the <see cref="AuditLogCacheKeyService.ClearTrackedKeys"/> method
        ///     removes all previously tracked audit log cache keys from the internal collection.
        /// </summary>
        [Fact]
        public void ClearTrackedKeys_RemovesAllKeys()
        {
            // Arrange
            _service.GetAuditLogListKey(1, 10, null);
            _service.GetAuditLogListKey(2, 10, AuditAction.SlowPerformance);

            // Act
            _service.ClearTrackedKeys();
            var keys = _service.GetAllLogListKeys();

            // Assert
            Assert.Empty(keys);
        }
    }
}
