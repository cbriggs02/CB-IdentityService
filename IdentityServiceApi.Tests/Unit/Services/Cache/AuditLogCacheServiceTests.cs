using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.CacheKeys;
using IdentityServiceApi.Services.Cache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace IdentityServiceApi.Tests.Unit.Services.Cache
{
    /// <summary>
    ///     Unit tests for the <see cref="AuditLogCacheService"/> class.
    ///     This class contains test cases for various audit log cache service scenarios, verifying the 
    ///     behavior of the audit log cache service functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class AuditLogCacheServiceTests
    {
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly Mock<ILogger<AuditLogCacheService>> _loggerMock;
        private readonly Mock<IAuditLogCacheKeyService> _auditLogCacheKeyMock;
        private readonly AuditLogCacheService _cacheService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuditLogCacheServiceTests"/> class,
        ///     setting up mocked dependencies and creating an instance of <see cref="AuditLogCacheService"/>.
        /// </summary>
        public AuditLogCacheServiceTests()
        {
            _cacheMock = new Mock<IMemoryCache>();
            _loggerMock = new Mock<ILogger<AuditLogCacheService>>();
            _auditLogCacheKeyMock = new Mock<IAuditLogCacheKeyService>();
            _cacheService = new AuditLogCacheService(_cacheMock.Object, _auditLogCacheKeyMock.Object, _loggerMock.Object);
        }

        /// <summary>
        ///     Verifies that the <see cref="AuditLogCacheService.ClearLogListCache"/> method
        ///     correctly removes all audit log list keys and calls <see cref="IAuditLogCacheKeyService.ClearTrackedKeys"/>
        ///     when no exceptions occur.
        /// </summary>
        [Fact]
        public void ClearLogListCache_SuccessfullyClearsKeysAndTrackedKeys()
        {
            // Arrange
            var keys = new List<string> { "key1", "key2" };

            _auditLogCacheKeyMock
                .Setup(k => k.GetAllLogListKeys())
                .Returns(keys);

            _cacheMock
                .Setup(c => c.Remove(It.IsAny<object>()));

            // Act
            _cacheService.ClearLogListCache();

            // Assert
            foreach (var key in keys)
            {
                _cacheMock.Verify(c => c.Remove(key), Times.Once);
            }

            _auditLogCacheKeyMock.Verify(k => k.ClearTrackedKeys(), Times.Once);

            // Verify no warnings were logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        /// <summary>
        ///     Verifies that the <see cref="AuditLogCacheService.ClearLogListCache"/> method
        ///     logs a warning when an exception occurs during the removal of audit log list cache keys.
        /// </summary>
        [Fact]
        public void ClearLogListCache_RemoveThrows_ExceptionIsCaughtAndLogged()
        {
            // Arrange
            var keys = new List<string> { "key1", "key2" };

            _auditLogCacheKeyMock
                .Setup(k => k.GetAllLogListKeys())
                .Returns(keys);
            _cacheMock
                .Setup(c => c.Remove(It.IsAny<object>()))
                .Throws(new InvalidOperationException("Mocked exception in Remove"));

            // Act
            _cacheService.ClearLogListCache();

            // Assert
            _cacheMock.Verify(c => c.Remove(It.IsAny<object>()), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains(ErrorMessages.AuditLogCache.FailedToClearLogListCache)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="AuditLogCacheService.ClearLogListCache"/> method
        ///     logs a warning when an exception occurs during clearing of tracked keys.
        /// </summary>
        [Fact]
        public void ClearLogListCache_ClearTrackedKeysThrows_ExceptionIsCaughtAndLogged()
        {
            // Arrange
            var keys = new List<string> { "key1", "key2" };

            _auditLogCacheKeyMock
                .Setup(k => k.GetAllLogListKeys())
                .Returns(keys);
            _cacheMock
                .Setup(c => c.Remove(It.IsAny<object>()));
            _auditLogCacheKeyMock
                .Setup(k => k.ClearTrackedKeys())
                .Throws(new InvalidOperationException("Mocked exception in ClearTrackedKeys"));

            // Act
            _cacheService.ClearLogListCache();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains(ErrorMessages.AuditLogCache.FailedToClearLogListCache)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _auditLogCacheKeyMock.Verify(k => k.ClearTrackedKeys(), Times.Once);
        }
    }
}
