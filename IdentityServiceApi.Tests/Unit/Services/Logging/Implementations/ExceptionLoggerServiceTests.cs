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
    ///     Unit tests for the <see cref="ExceptionLoggerService"/> class.
    ///     This class contains test cases for various audit exception logging scenarios, verifying the 
    ///     behavior of the exception logger functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class ExceptionLoggerServiceTests
    {
        private readonly Mock<IUserContextService> _userContextServiceMock;
        private readonly Mock<ILoggingValidator> _loggingValidatorMock;
        private readonly Mock<ApplicationDbContext> _dbContextMock;
        private readonly Mock<IParameterValidator> _parameterValidatorMock;
        private readonly Mock<IServiceResultFactory> _serviceResultFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ExceptionLoggerService _exceptionLoggerService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExceptionLoggerServiceTests"/> class.
        /// </summary>
        public ExceptionLoggerServiceTests()
        {
            _userContextServiceMock = new Mock<IUserContextService>();
            _loggingValidatorMock = new Mock<ILoggingValidator>();
            _dbContextMock = new Mock<ApplicationDbContext>();
            _parameterValidatorMock = new Mock<IParameterValidator>();
            _serviceResultFactoryMock = new Mock<IServiceResultFactory>();
            _mapperMock = new Mock<IMapper>();
            _exceptionLoggerService = new ExceptionLoggerService(_userContextServiceMock.Object, _loggingValidatorMock.Object, _dbContextMock.Object, _parameterValidatorMock.Object, _serviceResultFactoryMock.Object, _mapperMock.Object);
        }

        /// <summary>
        ///     Validates that the <see cref="ExceptionLoggerService"/> constructor throws 
        ///     an <see cref="ArgumentNullException"/> when null dependencies are provided.
        /// </summary>
        [Fact]
        public void ExceptionLoggerService_NullDependencies_ThrowsArgumentNullException()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ExceptionLoggerService(null, null, null, null, null, null));
        }

        /// <summary>
        ///     Verifies that the <see cref="ExceptionLoggerService.LogExceptionAsync(Exception)"/> method throws an 
        ///     ArgumentNullException when the provided exception is null.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task LogException_NullException_ThrowsArgumentNullException()
        {
            // Arrange
            Exception ex = null;

            _loggingValidatorMock
                .Setup(v => v.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _exceptionLoggerService.LogExceptionAsync(ex));

            _loggingValidatorMock.Verify(v => v.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="ExceptionLoggerService.LogExceptionAsync(Exception)"/> method  throws 
        ///     an InvalidOperationException when the context data is invalid.
        /// </summary>
        /// <param name="input">
        /// The invalid context data input (null or empty).
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task LogException_InvalidContextData_ThrowsInvalidOperationException(string input)
        {
            // Arrange
            ArrangeLoggingValidatorMock();
            ArrangeContextDataMock(input);
            ArrangeContextIpAddressMock(IPAddress.Parse("127.0.0.1"));

            Exception ex = new();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _exceptionLoggerService.LogExceptionAsync(ex));

            VerifyCallsToLoggingValidator(1);
            VerifyCallsToUserContextService();
        }

        /// <summary>
        ///     Verifies that the <see cref="ExceptionLoggerService.LogExceptionAsync(Exception)"/> method throws an
        ///     InvalidOperationException when the IP address is invalid.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task LogException_InvalidIpAddress_ThrowsInvalidOperationException()
        {
            // Arrange
            ArrangeLoggingValidatorMock();
            ArrangeContextDataMock("context data");
            ArrangeContextIpAddressMock(null); // Simulate invalid IP address

            Exception ex = new();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _exceptionLoggerService.LogExceptionAsync(ex));

            VerifyCallsToLoggingValidator(1);
            VerifyCallsToUserContextService();
        }

        /// <summary>
        ///     Unit test to verify that the <see cref="ExceptionLoggerService.LogExceptionAsync(Exception)"/> method 
        ///     successfully logs an exception under successful conditions, including logging the exception details 
        ///     and saving them to the audit log in the database.
        /// </summary>
        /// <returns>
        ///     A Task representing the asynchronous operation of the unit test.
        /// </returns>
        [Fact]
        public async Task LogException_SuccessConditions_SuccessfullyLogsException()
        {
            // Arrange
            ArrangeContextDataMock("context data");
            ArrangeContextIpAddressMock(IPAddress.Parse("127.0.0.1"));

            Exception ex = new("Test exception");

            _dbContextMock.Setup(s => s.AuditLogs.Add(It.IsAny<AuditLog>()));
            _dbContextMock.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            await _exceptionLoggerService.LogExceptionAsync(ex);

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

        private void ArrangeLoggingValidatorMock()
        {
            _loggingValidatorMock
                .Setup(v => v.ValidateContextData(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<InvalidOperationException>();
        }

        private void ArrangeContextIpAddressMock(IPAddress address)
        {
            _userContextServiceMock
                .Setup(ad => ad.GetAddress())
                .Returns(address);
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
        }
    }
}
