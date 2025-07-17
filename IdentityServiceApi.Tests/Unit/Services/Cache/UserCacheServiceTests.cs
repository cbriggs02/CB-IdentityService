using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.CacheKeys;
using IdentityServiceApi.Services.Cache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace IdentityServiceApi.Tests.Unit.Services.Cache
{
    /// <summary>
    ///     Unit tests for the <see cref="UserCacheService"/> class.
    ///     This class contains test cases for various user cache service scenarios, verifying the 
    ///     behavior of the user cache service functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class UserCacheServiceTests
    {
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly Mock<ILogger<UserCacheService>> _loggerMock;
        private readonly Mock<IUserCacheKeyService> _userCacheKeyMock;
        private readonly UserCacheService _cacheService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserCacheServiceTests"/> class,
        ///     setting up mocked dependencies and creating an instance of <see cref="UserCacheService"/>.
        /// </summary>
        public UserCacheServiceTests()
        {
            _cacheMock = new Mock<IMemoryCache>();
            _loggerMock = new Mock<ILogger<UserCacheService>>();
            _userCacheKeyMock = new Mock<IUserCacheKeyService>();
            _cacheService = new UserCacheService(_cacheMock.Object, _userCacheKeyMock.Object, _loggerMock.Object);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserCacheService"/> constructor throws
        ///     an <see cref="ArgumentNullException"/> when any required dependency is null.
        /// </summary> 
        [Fact]
        public void UserCacheService_NullDependencies_ThrowsArgumentNullException()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UserCacheService(null, null, null));
        }

        /// <summary>
        ///     Verifies that the <see cref="UserCacheService.ClearCreationStatsCache"/> method
        ///     correctly removes the creation stats cache key when no exception occurs.
        /// </summary>
        [Fact]
        public void ClearCreationStatsCache_SuccessfullyRemovesCache()
        {
            // Arrange
            _cacheMock
                .Setup(c => c.Remove(It.IsAny<object>()));

            // Act
            _cacheService.ClearCreationStatsCache();

            // Assert
            _cacheMock.Verify(c => c.Remove(It.IsAny<object>()), Times.Once);

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
        ///     Verifies that the <see cref="UserCacheService.ClearCreationStatsCache"/> method
        ///     logs a warning when an exception occurs while attempting to remove the cache entry.
        /// </summary>
        [Fact]
        public void ClearCreationStatsCache_ExceptionOccurs_ExceptionIsCaughtAndLogged()
        {
            // Arrange
            _cacheMock
                .Setup(c => c.Remove(It.IsAny<object>()))
                .Throws(new InvalidOperationException("Mocked exception"));

            // Act
            _cacheService.ClearCreationStatsCache();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains(ErrorMessages.UserCache.FailedToClearCreationStatsCache)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserCacheService.ClearStateMetricsCache"/> method
        ///     correctly removes the state metrics cache key when no exception occurs.
        /// </summary>
        [Fact]
        public void ClearStateMetricsCache_SuccessfullyRemovesCache()
        {
            // Arrange
            _cacheMock
                .Setup(c => c.Remove(It.IsAny<object>()));

            // Act
            _cacheService.ClearStateMetricsCache();

            // Assert
            _cacheMock.Verify(c => c.Remove(It.IsAny<object>()), Times.Once);

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
        ///     Verifies that the <see cref="UserCacheService.ClearStateMetricsCache"/> method
        ///     logs a warning when an exception occurs while removing the state metrics cache entry.
        /// </summary>
        [Fact]
        public void ClearStateMetricsCache_ExceptionOccurs_ExceptionIsCaughtAndLogged()
        {
            // Arrange
            _cacheMock
                .Setup(c => c.Remove(It.IsAny<object>()))
                .Throws(new InvalidOperationException("Mocked exception"));

            // Act
            _cacheService.ClearStateMetricsCache();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains(ErrorMessages.UserCache.FailedToClearStateMetricsCache)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserCacheService.ClearUserListCache"/> method
        ///     correctly removes all user list keys and calls <see cref="IUserCacheKeyService.ClearTrackedKeys"/>
        ///     when no exceptions occur.
        /// </summary>
        [Fact]
        public void ClearUserListCache_SuccessfullyClearsKeysAndTrackedKeys()
        {
            // Arrange
            var keys = new List<string> { "key1", "key2" };

            _userCacheKeyMock
                .Setup(k => k.GetAllUserListKeys())
                .Returns(keys);

            _cacheMock
                .Setup(c => c.Remove(It.IsAny<object>()));

            // Act
            _cacheService.ClearUserListCache();

            // Assert
            foreach (var key in keys)
            {
                _cacheMock.Verify(c => c.Remove(key), Times.Once);
            }

            _userCacheKeyMock.Verify(k => k.ClearTrackedKeys(), Times.Once);

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
        ///     Verifies that the <see cref="UserCacheService.ClearUserListCache"/> method
        ///     logs a warning when an exception occurs during the removal of user list cache keys.
        /// </summary>
        [Fact]
        public void ClearUserListCache_RemoveThrows_ExceptionIsCaughtAndLogged()
        {
            // Arrange
            var keys = new List<string> { "key1", "key2" };

            _userCacheKeyMock
                .Setup(k => k.GetAllUserListKeys())
                .Returns(keys);
            _cacheMock
                .Setup(c => c.Remove(It.IsAny<object>()))
                .Throws(new InvalidOperationException("Mocked exception in Remove"));

            // Act
            _cacheService.ClearUserListCache();

            // Assert
            _cacheMock.Verify(c => c.Remove(It.IsAny<object>()), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains(ErrorMessages.UserCache.FailedToClearUserListCache)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserCacheService.ClearUserListCache"/> method
        ///     logs a warning when an exception occurs during clearing of tracked keys.
        /// </summary>
        [Fact]
        public void ClearUserListCache_ClearTrackedKeysThrows_ExceptionIsCaughtAndLogged()
        {
            // Arrange
            var keys = new List<string> { "key1", "key2" };

            _userCacheKeyMock
                .Setup(k => k.GetAllUserListKeys())
                .Returns(keys);
            _cacheMock
                .Setup(c => c.Remove(It.IsAny<object>()));
            _userCacheKeyMock
                .Setup(k => k.ClearTrackedKeys())
                .Throws(new InvalidOperationException("Mocked exception in ClearTrackedKeys"));

            // Act
            _cacheService.ClearUserListCache();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains(ErrorMessages.UserCache.FailedToClearUserListCache)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _userCacheKeyMock.Verify(k => k.ClearTrackedKeys(), Times.Once);
        }
    }
}
