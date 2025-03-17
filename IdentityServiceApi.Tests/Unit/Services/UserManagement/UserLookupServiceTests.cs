using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.ServiceResultModels.UserManagement;
using IdentityServiceApi.Services.UserManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace IdentityServiceApi.Tests.Unit.Services.UserManagement
{
    /// <summary>
    ///     Unit tests for the <see cref="UserLookupService"/> class.
    ///     This class contains test cases for various user lookup scenarios, verifying the 
    ///     behavior of the user lookup functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class UserLookupServiceTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<ILogger<UserManager<User>>> _userManagerLoggerMock;
        private readonly Mock<IUserStore<User>> _userStoreMock;
        private readonly Mock<IOptions<IdentityOptions>> _optionsMock;
        private readonly Mock<IPasswordHasher<User>> _userHasherMock;
        private readonly Mock<IUserValidator<User>> _userValidatorMock;
        private readonly Mock<IPasswordValidator<User>> _passwordValidatorsMock;
        private readonly Mock<ILookupNormalizer> _keyNormalizerMock;
        private readonly Mock<IdentityErrorDescriber> _errorsMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IUserLookupServiceResultFactory> _lookupResultFactoryMock;
        private readonly Mock<IParameterValidator> _parameterValidatorMock;
        private readonly UserLookupService _userLookupService;
        private const string Username = "user-123";
        private const string UserId = "id-123";

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserLookupServiceTests"/> class.
        ///     This constructor sets up the mocked dependencies and creates an instance 
        ///     of the <see cref="UserLookupService"/> for testing.
        /// </summary>
        public UserLookupServiceTests()
        {
            _userStoreMock = new Mock<IUserStore<User>>();
            _optionsMock = new Mock<IOptions<IdentityOptions>>();
            _userHasherMock = new Mock<IPasswordHasher<User>>();
            _userValidatorMock = new Mock<IUserValidator<User>>();
            _passwordValidatorsMock = new Mock<IPasswordValidator<User>>();
            _keyNormalizerMock = new Mock<ILookupNormalizer>();
            _errorsMock = new Mock<IdentityErrorDescriber>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _userManagerLoggerMock = new Mock<ILogger<UserManager<User>>>();

            _userManagerMock = new Mock<UserManager<User>>(
                _userStoreMock.Object,
                _optionsMock.Object,
                _userHasherMock.Object,
                new[] { _userValidatorMock.Object },
                new[] { _passwordValidatorsMock.Object },
                _keyNormalizerMock.Object,
                 _errorsMock.Object,
                _serviceProviderMock.Object,
                _userManagerLoggerMock.Object
            );

            _lookupResultFactoryMock = new Mock<IUserLookupServiceResultFactory>();
            _parameterValidatorMock = new Mock<IParameterValidator>();
            _userLookupService = new UserLookupService(_userManagerMock.Object, _lookupResultFactoryMock.Object, _parameterValidatorMock.Object);
        }

        /// <summary>
        ///     Tests that an <see cref="ArgumentNullException"/> is thrown when <see cref="UserLookupService"/> is 
        ///     instantiated with a null dependencies.
        /// </summary>
        [Fact]
        public void UserLookupService_NullDependencies_ThrowsArgumentNullException()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UserLookupService(null, null, null));
        }

        /// <summary>
        ///     Verifies that the <see cref="UserLookupService.FindUserById"/> method throws an 
        ///     <see cref="ArgumentNullException"/> when provided with an invalid user ID.
        /// </summary>
        /// <param name="input">
        ///     The user ID input to be tested. This can be <c>null</c>, an empty string, or a whitespace-only string.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task FindUserById_InvalidUserId_ThrowsArgumentNullException(string input)
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userLookupService.FindUserById(input));

            VerifyCallsToParameterValidator();
        }

        /// <summary>
        ///     Verifies that the <see cref="UserLookupService.FindUserById"/> method returns 
        ///     a failure result when the specified user does not exist.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task FindUserById_UserDoesNotExist_ReturnsNotFoundFailureResult()
        {
            // Arrange
            const string ExpectedErrorMessage = ErrorMessages.User.NotFound;

            ArrangeUserLookupFailureServiceResult(ExpectedErrorMessage);

            _userManagerMock
                .Setup(f => f.FindByIdAsync(UserId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userLookupService.FindUserById(UserId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToParameterValidator();
            VerifyCallsToFindByIdAsync(UserId);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserLookupService.FindUserById"/> method returns 
        ///     a success result when the specified user exists.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task FindUserById_UserExist_ReturnsSuccessResult()
        {
            // Arrange
            var user = ArrangeMockUser();

            ArrangeUserLookupSuccessServiceResult(user);

            _userManagerMock
                .Setup(f => f.FindByIdAsync(UserId))
                .ReturnsAsync(user);

            // Act
            var result = await _userLookupService.FindUserById(UserId);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.UserFound);
            Assert.True(result.Success);

            VerifyCallsToParameterValidator();
            VerifyCallsToFindByIdAsync(UserId);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserLookupService.FindUserByUsername"/> method 
        ///     throws an <see cref="ArgumentNullException"/> when the provided username is null or empty.
        /// </summary>
        /// <param name="input">
        ///     The username input to be tested, which can be null, empty, or whitespace.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task FindUserByUsername_InvalidUserId_ThrowsArgumentNullException(string input)
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userLookupService.FindUserByUsername(input));

            VerifyCallsToParameterValidator();
        }

        /// <summary>
        ///     Verifies that the <see cref="UserLookupService.FindUserByUsername"/> method returns 
        ///     a not found failure result when the specified user does not exist.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task FindUserByUsername_UserDoesNotExist_ReturnsNotFoundFailureResult()
        {
            // Arrange
            const string ExpectedErrorMessage = ErrorMessages.User.NotFound;

            ArrangeUserLookupFailureServiceResult(ExpectedErrorMessage);

            _userManagerMock
                .Setup(f => f.FindByNameAsync(Username))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userLookupService.FindUserByUsername(Username);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToParameterValidator();
            VerifyCallsToFindByNameAsync(Username);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserLookupService.FindUserByUsername"/> method returns 
        ///     a success result when the specified user exists.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task FindUserByUsername_UserExist_ReturnsSuccessResult()
        {
            // Arrange
            var user = ArrangeMockUser();

            ArrangeUserLookupSuccessServiceResult(user);

            _userManagerMock
                .Setup(f => f.FindByNameAsync(Username))
                .ReturnsAsync(user);

            // Act
            var result = await _userLookupService.FindUserByUsername(Username);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.UserFound);
            Assert.True(result.Success);

            VerifyCallsToParameterValidator();
            VerifyCallsToFindByNameAsync(Username);
        }

        private static User ArrangeMockUser()
        {
            return new User { Id = UserId, UserName = Username };
        }

        private void ArrangeUserLookupSuccessServiceResult(User user)
        {
            var result = new UserLookupServiceResult
            {
                Success = true,
                UserFound = user,
            };

            _lookupResultFactoryMock
                .Setup(x => x.UserLookupOperationSuccess(user))
                .Returns(result);
        }

        private void ArrangeUserLookupFailureServiceResult(string expectedErrorMessage)
        {
            var result = new UserLookupServiceResult
            {
                Success = false,
                Errors = new List<string> { expectedErrorMessage }
            };

            _lookupResultFactoryMock
                .Setup(x => x.UserLookupOperationFailure(new[] { expectedErrorMessage }))
                .Returns(result);
        }

        private void VerifyCallsToParameterValidator()
        {
            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        private void VerifyCallsToFindByIdAsync(string UserId)
        {
            _userManagerMock.Verify(f => f.FindByIdAsync(UserId), Times.Once());
        }

        private void VerifyCallsToFindByNameAsync(string Username)
        {
            _userManagerMock.Verify(f => f.FindByNameAsync(Username), Times.Once());
        }
    }
}
