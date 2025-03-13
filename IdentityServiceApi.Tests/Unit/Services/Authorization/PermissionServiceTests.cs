using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Authorization;
using IdentityServiceApi.Interfaces.Logging;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.ServiceResultModels.Shared;
using IdentityServiceApi.Services.Authorization;
using Moq;

namespace IdentityServiceApi.Tests.Unit.Services.Authorization
{
    /// <summary>
    ///     Unit tests for the <see cref="PermissionService"/> class.
    ///     This class contains test cases for various permission scenarios, verifying the 
    ///     behavior of the permission functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class PermissionServiceTests
    {
        private readonly Mock<IAuthorizationService> _authServiceMock;
        private readonly Mock<ILoggerService> _loggerServiceMock;
        private readonly Mock<IParameterValidator> _parameterValidatorMock;
        private readonly Mock<IServiceResultFactory> _serviceResultFactoryMock;
        private readonly PermissionService _permissionService;
        private const string UserId = "test-id";

        /// <summary>
        ///     Initializes a new instance of the <see cref="PermissionServiceTests"/> class
        ///     and sets up necessary mock dependencies.
        /// </summary>
        public PermissionServiceTests()
        {
            _authServiceMock = new Mock<IAuthorizationService>();
            _loggerServiceMock = new Mock<ILoggerService>();
            _parameterValidatorMock = new Mock<IParameterValidator>();
            _serviceResultFactoryMock = new Mock<IServiceResultFactory>();

            _permissionService = new PermissionService(_authServiceMock.Object, _loggerServiceMock.Object, _parameterValidatorMock.Object, _serviceResultFactoryMock.Object);
        }

        /// <summary>
        ///     Tests that an <see cref="ArgumentNullException"/> is thrown when <see cref="PermissionService"/> is 
        ///     instantiated with a null dependencies.
        /// </summary>
        [Fact]
        public void PermissionService_NullDependencies_ThrowsArgumentNullException()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PermissionService(null, null, null, null));
        }

        /// <summary>
        ///     Verifies that calling the validate permissions method with null, empty or whitespace id throws an ArgumentNullException.
        /// </summary>
        /// <param name="id">
        ///     Used to test for invalid data like ( null, empty or whitespace )
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous test operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ValidatePermission_NullAndEmptyId_ReturnsArgumentNullException(string id)
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _permissionService.ValidatePermissions(id));

            VerifyCallsToParameterService();
        }

        /// <summary>
        ///     Tests the validate permissions method to verify that it returns a failure result
        ///     and logs an authorization breach when the user lacks necessary permissions.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermissions_UserLacksPermission_ReturnsFailureAndLogsBreach()
        {
            // Arrange
            const string expectedErrorMessage = ErrorMessages.Authorization.Forbidden;

            ArrangeAuthorizationServiceMock(false);

            _serviceResultFactoryMock
                .Setup(s => s.GeneralOperationFailure(It.IsAny<string[]>()))
                .Returns(new ServiceResult
                {
                    Success = false,
                    Errors = { expectedErrorMessage }
                });

            // Act
            var result = await _permissionService.ValidatePermissions(UserId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            _loggerServiceMock.Verify(l => l.LogAuthorizationBreach(), Times.Once);
            VerifyCallsToParameterService();
        }

        /// <summary>
        ///     Tests the validate permissions method to confirm it returns a success result
        ///     when the user has the required permissions.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermissions_UserHasPermission_ReturnsSuccess()
        {
            // Arrange
            ArrangeAuthorizationServiceMock(true);

            _serviceResultFactoryMock
                .Setup(s => s.GeneralOperationSuccess())
                .Returns(new ServiceResult { Success = true });

            // Act
            var result = await _permissionService.ValidatePermissions(UserId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            _loggerServiceMock.VerifyNoOtherCalls();
            VerifyCallsToParameterService();
        }

        private void ArrangeAuthorizationServiceMock(bool hasPermission)
        {
            _authServiceMock
                .Setup(a => a.ValidatePermission(UserId))
                .ReturnsAsync(hasPermission);
        }

        private void VerifyCallsToParameterService()
        {
            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
