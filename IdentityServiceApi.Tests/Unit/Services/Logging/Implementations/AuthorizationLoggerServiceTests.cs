using AutoMapper;
using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.Authentication;
using IdentityServiceApi.Interfaces.Logging;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Services.Logging.Implementations;
using Moq;
using System.Net;
using System.Security.Claims;

namespace IdentityServiceApi.Tests.Unit.Services.Logging.Implementations
{
    /// <summary>
    ///     Unit tests for the <see cref="AuthorizationLoggerService"/> class.
    ///     This class contains test cases for various audit auth logging scenarios, verifying the 
    ///     behavior of the auth logger functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class AuthorizationLoggerServiceTests
    {
        private readonly Mock<IUserContextService> _userContextServiceMock;
        private readonly Mock<ILoggingValidator> _loggingValidatorMock;
        private readonly Mock<ApplicationDbContext> _dbContextMock;
        private readonly Mock<IParameterValidator> _parameterValidatorMock;
        private readonly Mock<IServiceResultFactory> _serviceResultFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AuthorizationLoggerService _authorizationLoggerService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthorizationLoggerServiceTests"/> class.
        ///     Sets up mock dependencies and an instance of the <see cref="AuthorizationLoggerService"/> for testing.
        /// </summary>
        public AuthorizationLoggerServiceTests()
        {
            _userContextServiceMock = new Mock<IUserContextService>();
            _loggingValidatorMock = new Mock<ILoggingValidator>();
            _dbContextMock = new Mock<ApplicationDbContext>();
            _parameterValidatorMock = new Mock<IParameterValidator>();
            _serviceResultFactoryMock = new Mock<IServiceResultFactory>();
            _mapperMock = new Mock<IMapper>();
            _authorizationLoggerService = new AuthorizationLoggerService(_userContextServiceMock.Object, _loggingValidatorMock.Object, _dbContextMock.Object, _parameterValidatorMock.Object, _serviceResultFactoryMock.Object, _mapperMock.Object);
        }

        /// <summary>
        ///     Tests the behavior of the <see cref="AuthorizationLoggerService"/> constructor when null dependencies are provided.
        ///     Ensures an <see cref="ArgumentNullException"/> is thrown.
        /// </summary>
        [Fact]
        public void AuthorizationLoggerService_NullDependencies_ThrowsArgumentNullException()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AuthorizationLoggerService(null, null, null, null, null, null));
        }

        /// <summary>
        ///     Tests the <see cref="AuthorizationLoggerService.LogAuthorizationBreach"/> method with invalid context data.
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
        public async Task LogAuthorizationBreach_InvalidContextData_ThrowsInvalidOperationException(string input)
        {
            // Arrange
            ArrangeLoggingValidatorMock();
            ArrangeContextDataMock(input);
            ArrangeContextIpAddressMock(IPAddress.Parse("127.0.0.1"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _authorizationLoggerService.LogAuthorizationBreach());

            VerifyCallsToLoggingValidator(1);
            VerifyCallsToUserContextService();
        }

        /// <summary>
        ///     Tests the <see cref="AuthorizationLoggerService.LogAuthorizationBreach"/> method with an invalid IP address.
        ///     Verifies that an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task LogAuthorizationBreach_InvalidIpAddress_ThrowsInvalidOperationException()
        {
            // Arrange
            ArrangeLoggingValidatorMock();
            ArrangeContextDataMock("context data");
            ArrangeContextIpAddressMock(null); // Simulate invalid IP address

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _authorizationLoggerService.LogAuthorizationBreach());

            VerifyCallsToLoggingValidator(1);
            VerifyCallsToUserContextService();
        }

        /// <summary>
        ///     Tests the <see cref="AuthorizationLoggerService.LogAuthorizationBreach"/> method under valid conditions.
        ///     Verifies that the authorization breach is successfully logged.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task LogAuthorizationBreach_SuccessfulConditions_SuccessfullyLogsAuthorizationBreach()
        {
            // Arrange
            ArrangeContextDataMock("Valid User Data");
            ArrangeContextIpAddressMock(IPAddress.Parse("127.0.0.1"));

            _dbContextMock.Setup(s => s.AuditLogs.Add(It.IsAny<AuditLog>()));
            _dbContextMock.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            await _authorizationLoggerService.LogAuthorizationBreach();

            // Assert
            VerifyCallsToUserContextService();

            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
