using AutoMapper;
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
    ///     Unit tests for the <see cref="ExceptionLoggerService"/> class.
    ///     This class contains test cases for various audit exception logging scenarios, verifying the 
    ///     behavior of the exception logger functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    [Trait("Category", "UnitTest")]
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
        ///     Verifies that LogException throws an ArgumentNullException when the provided exception is null.
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
            await Assert.ThrowsAsync<ArgumentNullException>(() => _exceptionLoggerService.LogException(ex));

            _loggingValidatorMock.Verify(v => v.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        ///     Verifies that LogException throws an InvalidOperationException when the context data is invalid.
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
            await Assert.ThrowsAsync<InvalidOperationException>(() => _exceptionLoggerService.LogException(ex));

            VerifyCallsToLoggingValidator(1);
            VerifyCallsToUserContextService();
        }

        /// <summary>
        ///     Verifies that LogException throws an InvalidOperationException when the IP address is invalid.
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
            await Assert.ThrowsAsync<InvalidOperationException>(() => _exceptionLoggerService.LogException(ex));

            VerifyCallsToLoggingValidator(1);
            VerifyCallsToUserContextService();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LogException_SuccessConditions_SuccessfullyLogsException()
        {
            ArrangeContextDataMock("context data");
            ArrangeContextIpAddressMock(IPAddress.Parse("127.0.0.1"));

            Exception ex = new("Test exception");

            await _exceptionLoggerService.LogException(ex);

            VerifyCallsToUserContextService();
        }

        /// <summary>
        ///     Sets up the mock behavior for retrieving user context data.
        /// </summary>
        /// <param name="input">
        ///     The context data to be returned by the mock setup.
        /// </param>
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

        /// <summary>
        ///     Sets up the mock behavior for validating context data.
        /// </summary>
        private void ArrangeLoggingValidatorMock()
        {
            _loggingValidatorMock
                .Setup(v => v.ValidateContextData(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<InvalidOperationException>();
        }

        /// <summary>
        ///     Sets up the mock behavior for retrieving the user's IP address.
        /// </summary>
        /// <param name="address">
        ///     The IP address to be returned by the mock setup.
        /// </param>
        private void ArrangeContextIpAddressMock(IPAddress address)
        {
            _userContextServiceMock
                .Setup(ad => ad.GetAddress())
                .Returns(address);
        }

        /// <summary>
        ///     Verifies the number of times the logging validator was called.
        /// </summary>
        /// <param name="numOfTimes">
        ///     The expected number of times the validator method was called.
        /// </param>
        private void VerifyCallsToLoggingValidator(int numOfTimes)
        {
            _loggingValidatorMock.Verify(v => v.ValidateContextData(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(numOfTimes));
        }

        /// <summary>
        ///     Verifies the number of times methods on the user context service were called.
        /// </summary>
        private void VerifyCallsToUserContextService()
        {
            _userContextServiceMock.Verify(p => p.GetClaimsPrincipal(), Times.Once());
            _userContextServiceMock.Verify(id => id.GetUserId(It.IsAny<ClaimsPrincipal>()), Times.Once());
            _userContextServiceMock.Verify(ad => ad.GetAddress(), Times.Once());
        }
    }
}
