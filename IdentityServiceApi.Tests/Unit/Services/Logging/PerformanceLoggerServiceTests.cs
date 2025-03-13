using AutoMapper;
using IdentityServiceApi.Constants;
using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.Authentication;
using IdentityServiceApi.Interfaces.Logging;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Services.Logging.Implementations;
using Moq;
using System.Net;
using System.Security.Claims;

namespace IdentityServiceApi.Tests.Unit.Services.Logging
{
    /// <summary>
    ///     Unit tests for the <see cref="PerformanceLoggerService"/> class.
    ///     This class contains test cases for various audit performance logging scenarios, verifying the 
    ///     behavior of the performance logger functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class PerformanceLoggerServiceTests
    {
        private readonly Mock<IUserContextService> _userContextServiceMock;
        private readonly Mock<ILoggingValidator> _loggingValidatorMock;
        private readonly Mock<ApplicationDbContext> _dbContextMock;
        private readonly Mock<IParameterValidator> _parameterValidatorMock;
        private readonly Mock<IServiceResultFactory> _serviceResultFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly PerformanceLoggerService _performanceLoggerService;
        private const long ValidResponseTime = 100;


        /// <summary>
        ///     Initializes a new instance of the <see cref="PerformanceLoggerServiceTests"/> class.
        /// </summary>
        public PerformanceLoggerServiceTests()
        {
            _userContextServiceMock = new Mock<IUserContextService>();
            _loggingValidatorMock = new Mock<ILoggingValidator>();
            _dbContextMock = new Mock<ApplicationDbContext>();
            _parameterValidatorMock = new Mock<IParameterValidator>();
            _serviceResultFactoryMock = new Mock<IServiceResultFactory>();
            _mapperMock = new Mock<IMapper>();

            _performanceLoggerService = new PerformanceLoggerService(_userContextServiceMock.Object, _loggingValidatorMock.Object, _dbContextMock.Object, _parameterValidatorMock.Object, _serviceResultFactoryMock.Object, _mapperMock.Object);
        }

        /// <summary>
        ///     Validates that the <see cref="PerformanceLoggerService"/> constructor throws 
        ///     an <see cref="ArgumentNullException"/> when null dependencies are provided.
        /// </summary>
        [Fact]
        public void PerformanceLoggerService_NullDependencies_ThrowsArgumentNullException()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PerformanceLoggerService(null, null, null, null, null, null));
        }

        /// <summary>
        ///     Tests that the <see cref="PerformanceLoggerService.LogSlowPerformance"/> method 
        ///     throws an <see cref="ArgumentException"/> when an invalid response time is provided.
        /// </summary>
        /// <param name="responseTime">
        ///     The response time value to test, which is expected to be invalid.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous unit test operation.
        /// </returns>
        [Theory]
        [InlineData(0)]
        [InlineData(-20)]
        [InlineData(-111)]
        [InlineData(-12121)]
        [InlineData(-45745)]
        [InlineData(-777777777777)]
        public async Task LogSlowPerformance_InvalidResponseTime_ThrowsArgumentException(long responseTime)
        {
            // Arrange
            const string ExpectedExceptionMessage = ErrorMessages.AuditLog.PerformanceLog.InvalidResponseTime;

            //Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _performanceLoggerService.LogSlowPerformance(responseTime));

            Assert.Equal(ExpectedExceptionMessage, exception.Message);
        }

        /// <summary>
        ///     Tests the <see cref="PerformanceLoggerService.LogSlowPerformance"/> method with invalid context data.
        ///     Verifies that an <see cref="InvalidOperationException"/> is thrown for invalid user context data.
        /// </summary>
        /// <param name="input">
        ///     The invalid user context data to test.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task LogSlowPerformance_InvalidContextData_ThrowsInvalidOperationException(string input)
        {
            // Arrange
            ArrangeLoggingValidatorMock();
            ArrangeContextDataMock(input);
            ArrangeContextIpAddressMock(IPAddress.Parse("127.0.0.1"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _performanceLoggerService.LogSlowPerformance(ValidResponseTime));

            VerifyCallsToLoggingValidator(1);
            VerifyCallsToUserContextService();
        }

        /// <summary>
        ///     Tests the <see cref="PerformanceLoggerService.LogSlowPerformance"/> method with an invalid IP address.
        ///     Verifies that an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task LogSlowPerformance_InvalidIpAddress_ThrowsInvalidOperationException()
        {
            // Arrange
            ArrangeLoggingValidatorMock();
            ArrangeContextDataMock("context data");
            ArrangeContextIpAddressMock(null); // Simulate invalid IP address

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _performanceLoggerService.LogSlowPerformance(ValidResponseTime));

            VerifyCallsToLoggingValidator(1);
            VerifyCallsToUserContextService();
        }

        /// <summary>
        ///     Tests the <see cref="PerformanceLoggerService.LogSlowPerformance"/> method under valid conditions.
        ///     Verifies that the authorization breach is successfully logged.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task LogSlowPerformance_SuccessfulConditions_SuccessfullyLogsAuthorizationBreach()
        {
            // Arrange
            ArrangeContextDataMock("Valid User Data");
            ArrangeContextIpAddressMock(IPAddress.Parse("127.0.0.1"));

            // Act
            await _performanceLoggerService.LogSlowPerformance(ValidResponseTime);

            // Assert
            VerifyCallsToUserContextService();
        }

        private void ArrangeContextDataMock(string input)
        {
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.NameIdentifier, input)
            }));

            _userContextServiceMock
                .Setup(p => p.GetClaimsPrincipal())
                .Returns(claimsPrincipal);

            _userContextServiceMock
                .Setup(id => id.GetUserId(claimsPrincipal))
                .Returns(input);

            _userContextServiceMock
                .Setup(rp => rp.GetRequestPath())
                .Returns(input);
        }

        private void ArrangeContextIpAddressMock(IPAddress address)
        {
            _userContextServiceMock
                .Setup(ad => ad.GetAddress())
                .Returns(address);
        }

        private void ArrangeLoggingValidatorMock()
        {
            _loggingValidatorMock
                .Setup(v => v.ValidateContextData(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<InvalidOperationException>();
        }

        private void VerifyCallsToLoggingValidator(int numOfTimes)
        {
            _loggingValidatorMock.Verify(v => v.ValidateContextData(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(numOfTimes));
        }

        private void VerifyCallsToUserContextService()
        {
            _userContextServiceMock.Verify(p => p.GetClaimsPrincipal(), Times.Once());
            _userContextServiceMock.Verify(id => id.GetUserId(It.IsAny<ClaimsPrincipal>()), Times.Once());
            _userContextServiceMock.Verify(ad => ad.GetAddress(), Times.Once());
            _userContextServiceMock.Verify(rp => rp.GetRequestPath(), Times.Once());
        }
    }
}
