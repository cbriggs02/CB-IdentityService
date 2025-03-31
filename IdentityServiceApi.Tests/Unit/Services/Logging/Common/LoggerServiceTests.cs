using IdentityServiceApi.Interfaces.Logging;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Services.Logging.Common;
using Moq;

namespace IdentityServiceApi.Tests.Unit.Services.Logging.Common
{
    /// <summary>
    ///     Unit tests for the <see cref="LoggerService"/> class.
    ///     This class contains test cases for various logging scenarios, verifying the 
    ///     behavior of the logger functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class LoggerServiceTests
    {
        private readonly Mock<IAuthorizationLoggerService> _authorizationLoggerServiceMock;
        private readonly Mock<IExceptionLoggerService> _exceptionLoggerMock;
        private readonly Mock<IPerformanceLoggerService> _performanceLoggerMock;
        private readonly Mock<IParameterValidator> _parameterValidator;
        private readonly LoggerService _loggerService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LoggerServiceTests"/> class.
        ///     Sets up mock dependencies required by <see cref="LoggerService"/>.
        /// </summary>
        public LoggerServiceTests()
        {
            _authorizationLoggerServiceMock = new Mock<IAuthorizationLoggerService>();
            _exceptionLoggerMock = new Mock<IExceptionLoggerService>();
            _performanceLoggerMock = new Mock<IPerformanceLoggerService>();
            _parameterValidator = new Mock<IParameterValidator>();
            _loggerService = new LoggerService(_authorizationLoggerServiceMock.Object, _exceptionLoggerMock.Object, _performanceLoggerMock.Object, _parameterValidator.Object);

        }

        /// <summary>
        ///     Verifies that an <see cref="ArgumentNullException"/> is thrown when the 
        ///     <see cref="LoggerService"/> is instantiated with null dependencies.
        /// </summary>
        [Fact]
        public void LoggerService_NullDependencies_ThrowsArgumentNullException()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new LoggerService(null, null, null, null));
        }

        /// <summary>
        ///     Verifies that the <see cref="LoggerService.LogAuthorizationBreach"/> method
        ///     performs logging operations successfully under valid conditions.
        /// </summary>
        [Fact]
        public async Task LogAuthorizationBreach_SuccessfulConditions_SuccessfulLogsAuthorizationBreach()
        {
            // Act
            await _loggerService.LogAuthorizationBreach();

            // Assert
            _authorizationLoggerServiceMock.Verify(v => v.LogAuthorizationBreach(), Times.Once());
        }

        /// <summary>
        ///     Verifies that the <see cref="LoggerService.LogException"/> method throws
        ///     an <see cref="ArgumentNullException"/> when called with a null exception.
        /// </summary>
        [Fact]
        public async Task LogException_NullException_ThrowsArgumentNullException()
        {
            // Arrange
            _parameterValidator
                .Setup(v => v.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _loggerService.LogException(null));

            _parameterValidator.Verify(v => v.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()));
        }

        /// <summary>
        ///     Verifies that the <see cref="LoggerService.LogException"/> method performs
        ///     logging operations successfully under valid conditions.
        /// </summary>
        [Fact]
        public async Task LogException_ValidException_SuccessfullyLogsException()
        {
            // Arrange
            Exception ex = new();

            // Act
            await _loggerService.LogException(ex);

            // Assert
            _parameterValidator.Verify(v => v.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()), Times.Once());
            _exceptionLoggerMock.Verify(v => v.LogException(ex), Times.Once());
        }

        /// <summary>
        ///     Verifies that the <see cref="LoggerService.LogSlowPerformance"/> method
        ///     performs logging operations successfully for various response times.
        /// </summary>
        /// <param name="responseTime">
        ///     The response time value to log.
        /// </param>
        [Theory]
        [InlineData(111111111111)]
        [InlineData(5)]
        [InlineData(-50000000000)]
        [InlineData(40)]
        [InlineData(500)]
        [InlineData(-200)]
        public async Task LogSlowPerformance_SuccessfulConditions_SuccessfullyLogsSlowPerformance(long responseTime)
        {
            // Act 
            await _loggerService.LogSlowPerformance(responseTime);

            // Assert
            _performanceLoggerMock.Verify(v => v.LogSlowPerformance(responseTime), Times.Once());
        }
    }
}
