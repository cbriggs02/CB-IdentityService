using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Authorization;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.RequestModels.UserManagement;
using IdentityServiceApi.Models.ServiceResultModels.Shared;
using IdentityServiceApi.Models.ServiceResultModels.UserManagement;
using IdentityServiceApi.Services.UserManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace IdentityServiceApi.Tests.Unit.Services.UserManagement
{
    /// <summary>
    ///     Unit tests for the <see cref="PasswordService"/> class.
    ///     This class contains test cases for various password scenarios, verifying the 
    ///     behavior of the password functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class PasswordServiceTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IPasswordHistoryService> _passwordHistoryServiceMock;
        private readonly Mock<IPermissionService> _permissionServiceMock;
        private readonly Mock<IParameterValidator> _parameterValidatorMock;
        private readonly Mock<IServiceResultFactory> _serviceResultFactoryMock;
        private readonly Mock<IUserLookupService> _userLookupServiceMock;
        private readonly Mock<ILogger<UserManager<User>>> _userManagerLoggerMock;
        private readonly Mock<IUserStore<User>> _userStoreMock;
        private readonly Mock<IOptions<IdentityOptions>> _optionsMock;
        private readonly Mock<IPasswordHasher<User>> _userHasherMock;
        private readonly Mock<IUserValidator<User>> _userValidatorMock;
        private readonly Mock<IPasswordValidator<User>> _passwordValidatorsMock;
        private readonly Mock<ILookupNormalizer> _keyNormalizerMock;
        private readonly Mock<IdentityErrorDescriber> _errorsMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly PasswordService _passwordService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PasswordServiceTests"/> class.
        ///     This constructor sets up the mocked dependencies and creates an instance 
        ///     of the <see cref="PasswordService"/> for testing.
        /// </summary>
        public PasswordServiceTests()
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

            _optionsMock.Setup(o => o.Value).Returns(new IdentityOptions());

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

            _passwordHistoryServiceMock = new Mock<IPasswordHistoryService>();
            _permissionServiceMock = new Mock<IPermissionService>();
            _parameterValidatorMock = new Mock<IParameterValidator>();
            _serviceResultFactoryMock = new Mock<IServiceResultFactory>();
            _userLookupServiceMock = new Mock<IUserLookupService>();

            _passwordService = new PasswordService(_userManagerMock.Object, _passwordHistoryServiceMock.Object, _permissionServiceMock.Object, _parameterValidatorMock.Object, _serviceResultFactoryMock.Object, _userLookupServiceMock.Object);
        }

        /// <summary>
        ///     Tests that an <see cref="ArgumentNullException"/> is thrown when <see cref="PasswordService"/> is 
        ///     instantiated with a null dependencies.
        /// </summary>
        [Fact]
        public void PasswordService_NullDependencies_ThrowsArgumentNullException()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PasswordService(null, null, null, null, null, null));
        }

        /// <summary>
        ///     Verifies that calling <see cref="PasswordService.SetPassword"/> with an invalid user ID 
        ///     (null, empty, or whitespace) results in an <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <param name="id">
        ///     The invalid user ID to be tested (null, empty, or whitespace).
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous unit test operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async Task SetPassword_InvalidId_ThrowsArgumentNullException(string id)
        {
            // Arrange 
            var request = ArrangeSetPasswordRequest();

            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act 
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => _passwordService.SetPassword(id, request));

            // Arrange
            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        /// <summary>
        ///     Verifies that calling <see cref="PasswordService.SetPassword"/> with a null request object 
        ///     results in an <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task SetPassword_NullRequestParameterObject_ThrowsArgumentNullException()
        {
            // Arrange 
            _parameterValidatorMock
                .Setup(x => x.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => _passwordService.SetPassword("id-123", null));

            VerifyCallsToParameterService(1);
        }

        /// <summary>
        ///     Verifies that calling <see cref="PasswordService.SetPassword"/> with an invalid request object 
        ///     containing a null, empty, or whitespace password results in an <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <param name="input">
        ///     The invalid password input (null, empty, or whitespace).
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous unit test operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async Task SetPassword_InvalidRequestObjectData_ThrowsArgumentNullException(string input)
        {
            // Arrange 
            var request = new SetPasswordRequest
            {
                Password = input,
                PasswordConfirmed = input
            };

            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => _passwordService.SetPassword("id-123", request));

            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        ///     Verifies that calling <see cref="PasswordService.SetPassword"/> with mismatched passwords 
        ///     results in a failure <see cref="ServiceResult"/>.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task SetPassword_PasswordDoesNotMatchPasswordConfirmed_ReturnsFailureResult()
        {
            // Arrange
            const string ExpectedErrorMessage = ErrorMessages.Password.Mismatch;

            var request = new SetPasswordRequest
            {
                Password = "password",
                PasswordConfirmed = "different password"
            };

            ArrangeFailureServiceResult(ExpectedErrorMessage);

            // Act
            var result = await _passwordService.SetPassword("id-123", request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToParameterService(3);
        }

        /// <summary>
        ///     Verifies that calling <see cref="PasswordService.SetPassword"/> with a non-existent user ID 
        ///     results in a failure <see cref="ServiceResult"/> with a "User Not Found" error.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task SetPassword_NonExistentUserId_ReturnsNotFoundFailureResult()
        {
            // Arrange 
            const string UserId = "non-existent-id";
            const string ExpectedErrorMessage = ErrorMessages.User.NotFound;

            var request = ArrangeSetPasswordRequest();

            ArrangeUserLookupServiceMock(null, UserId, ExpectedErrorMessage);
            ArrangeFailureServiceResult(ExpectedErrorMessage);

            // Act
            var result = await _passwordService.SetPassword(UserId, request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(UserId);
            VerifyCallsToParameterService(3);
        }

        /// <summary>
        ///     Tests the <see cref="PasswordService.SetPassword"/> method to ensure 
        ///     it returns a failure result when the password has already been set.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous unit test execution.
        /// </returns>
        [Fact]
        public async Task SetPassword_PasswordAlreadySet_ReturnsAlreadySetFailureResult()
        {
            // Arrange 
            const string UserId = "id-123";
            const string ExpectedErrorMessage = ErrorMessages.Password.AlreadySet;

            var request = ArrangeSetPasswordRequest();
            var user = ArrangeMockUser(UserId, request.Password);

            ArrangeUserLookupServiceMock(user, UserId, ExpectedErrorMessage);
            ArrangeFailureServiceResult(ExpectedErrorMessage);

            // Act
            var result = await _passwordService.SetPassword(UserId, request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(UserId);
            VerifyCallsToParameterService(3);
        }

        /// <summary>
        ///     Tests the <see cref="PasswordService.SetPassword"/> method to ensure 
        ///     it returns a general success result when all conditions are met.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous unit test execution.
        /// </returns>
        [Fact]
        public async Task SetPassword_SuccessfulConditions_ReturnsGeneralSuccessResult()
        {
            // Arrange 
            const string UserId = "id-123";

            var request = ArrangeSetPasswordRequest();
            var user = new User { Id = UserId, UserName = "user123", PasswordHash = null };

            ArrangeUserLookupServiceMock(user, UserId, "");

            _userManagerMock
                .Setup(a => a.AddPasswordAsync(user, request.Password))
                .ReturnsAsync(IdentityResult.Success);

            ArrangeSuccessServiceResult();

            // Act
            var result = await _passwordService.SetPassword(UserId, request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            VerifyCallsToLookupService(UserId);
            VerifyCallsToParameterService(3);

            _userManagerMock.Verify(a => a.AddPasswordAsync(user, request.Password), Times.Once);
        }

        /// <summary>
        ///     Tests that the <see cref="PasswordService.UpdatePassword"/> method throws an 
        ///     <see cref="ArgumentNullException"/> when the provided user ID is null, empty, 
        ///     or consists only of whitespace.
        /// </summary>
        /// <param name="id">
        ///     The user ID to be validated, which may be null, empty, or whitespace.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation of the test.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async Task UpdatePassword_InvalidId_ThrowsArgumentNullException(string id)
        {
            // Arrange 
            var request = ArrangeUpdatePasswordRequest();

            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act 
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => _passwordService.UpdatePassword(id, request));

            // Arrange
            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        /// <summary>
        ///     Tests that the <see cref="PasswordService.UpdatePassword"/> method throws an 
        ///     <see cref="ArgumentNullException"/> when the request parameter object is null.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation of the test.
        /// </returns>
        [Fact]
        public async Task UpdatePassword_NullRequestParameterObject_ThrowsArgumentNullException()
        {
            // Arrange 
            _parameterValidatorMock
                .Setup(x => x.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => _passwordService.UpdatePassword("id-123", null));

            VerifyCallsToParameterService(1);
        }

        /// <summary>
        ///     Tests that the <see cref="PasswordService.UpdatePassword"/> method throws an 
        ///     <see cref="ArgumentNullException"/> when provided with an invalid request object 
        ///     containing null, empty, or whitespace values.
        /// </summary>
        /// <param name="input">
        ///     The test input representing an invalid password value.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation of the test.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async Task UpdatePassword_InvalidRequestObjectData_ThrowsArgumentNullException(string input)
        {
            // Arrange 
            var request = new UpdatePasswordRequest
            {
                CurrentPassword = input,
                NewPassword = input
            };

            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => _passwordService.UpdatePassword("id-123", request));

            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        ///     Tests the <see cref="PasswordService.UpdatePassword"/> method to ensure that attempting 
        ///     to update another user's password results in a forbidden failure response.
        /// </summary>
        /// <param name="roleName">
        ///     The role of the user attempting to update another user's password.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous unit test execution.
        /// </returns>
        [Theory]
        [InlineData(Roles.Admin)]
        [InlineData(Roles.User)]
        public async Task UpdatePassword_TryingToUpdateOtherUserPassword_ReturnsForbiddenFailureResult(string roleName)
        {
            // Arrange
            const string ExpectedErrorMessage = ErrorMessages.Authorization.Forbidden;
            const string UserId = "id-123";
            const string OtherUserId = "id-999";

            var request = ArrangeUpdatePasswordRequest();
            var user = ArrangeMockUser(OtherUserId, request.CurrentPassword);

            _userManagerMock
                .Setup(a => a.AddToRoleAsync(user, roleName))
                .ReturnsAsync(IdentityResult.Success);
            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = false, Errors = new List<string> { ExpectedErrorMessage } });

            ArrangeFailureServiceResult(ExpectedErrorMessage);

            // Act
            var result = await _passwordService.UpdatePassword(UserId, request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            _permissionServiceMock.Verify(p => p.ValidatePermissions(UserId), Times.Once);
        }

        /// <summary>
        ///     Tests the <see cref="PasswordService.UpdatePassword"/> method to ensure that providing 
        ///     a non-existent user ID results in an invalid credentials failure response.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous unit test execution.
        /// </returns>
        [Fact]
        public async Task UpdatePassword_NonExistentUserId_ReturnsInvalidCredentialsFailureResult()
        {
            // Arrange 
            const string UserId = "non-existent-id";
            const string ExpectedErrorMessage = ErrorMessages.Password.InvalidCredentials;

            var request = ArrangeUpdatePasswordRequest();

            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = true });

            ArrangeUserLookupServiceMock(null, UserId, ExpectedErrorMessage);
            ArrangeFailureServiceResult(ExpectedErrorMessage);

            // Act
            var result = await _passwordService.UpdatePassword(UserId, request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(UserId);
            VerifyCallsToParameterService(3);
        }

        /// <summary>
        ///     Tests the <see cref="PasswordService.UpdatePassword"/> method to ensure that when a user 
        ///     is found but has a null password hash, the service returns an invalid credentials failure response.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous unit test execution.
        /// </returns>
        [Fact]
        public async Task UpdatePassword_UserFoundHasNullPasswordHash_ReturnsInvalidCredentialsFailureResult()
        {
            // Arrange 
            const string UserId = "id-123";
            const string ExpectedErrorMessage = ErrorMessages.Password.InvalidCredentials;

            var request = ArrangeUpdatePasswordRequest();
            var user = new User { Id = UserId, UserName = "user123", PasswordHash = null };

            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = true });

            ArrangeUserLookupServiceMock(user, UserId, "");
            ArrangeFailureServiceResult(ExpectedErrorMessage);

            // Act
            var result = await _passwordService.UpdatePassword(UserId, request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(UserId);
            VerifyCallsToParameterService(3);
        }

        /// <summary>
        ///     Tests the <see cref="PasswordService.UpdatePassword"/> method to verify that it returns
        ///     an invalid credentials failure result when the provided current password does not match 
        ///     the stored password.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous test execution.
        /// </returns>
        [Fact]
        public async Task UpdatePassword_CurrentPasswordDoesNotMatchStoredPassword_ReturnsInvalidCredentialsFailureResult()
        {
            // Arrange 
            const string UserId = "id-123";
            const string ExpectedErrorMessage = ErrorMessages.Password.InvalidCredentials;

            var request = ArrangeUpdatePasswordRequest();
            var user = ArrangeMockUser(UserId, request.CurrentPassword);

            _permissionServiceMock
              .Setup(p => p.ValidatePermissions(UserId))
              .ReturnsAsync(new ServiceResult { Success = true });

            ArrangeUserLookupServiceMock(user, UserId, "");

            _userManagerMock
                .Setup(c => c.CheckPasswordAsync(user, request.CurrentPassword))
                .ReturnsAsync(false);

            ArrangeFailureServiceResult(ExpectedErrorMessage);

            // Act
            var result = await _passwordService.UpdatePassword(UserId, request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(UserId);
            VerifyCallsToParameterService(3);

            _userManagerMock.Verify(c => c.CheckPasswordAsync(user, request.CurrentPassword), Times.Once());
        }

        /// <summary>
        ///     Tests the <see cref="PasswordService.UpdatePassword"/> method to ensure that when a new 
        ///     password has already been used by the user, it returns a failure result indicating that 
        ///     password reuse is not allowed.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task UpdatePassword_NewPasswordProvidedHasAlreadyBeenUsed_ReturnsCannotReuseFailureResult()
        {
            // Arrange 
            const string UserId = "id-123";
            const string ExpectedErrorMessage = ErrorMessages.Password.CannotReuse;

            var request = ArrangeUpdatePasswordRequest();
            var user = ArrangeMockUser(UserId, request.CurrentPassword);

            _permissionServiceMock
              .Setup(p => p.ValidatePermissions(UserId))
              .ReturnsAsync(new ServiceResult { Success = true });

            ArrangeUserLookupServiceMock(user, UserId, "");

            _userManagerMock
                .Setup(c => c.CheckPasswordAsync(user, request.CurrentPassword))
                .ReturnsAsync(true);
            _passwordHistoryServiceMock
                .Setup(f => f.FindPasswordHash(It.Is<SearchPasswordHistoryRequest>(
                    req => req.UserId == UserId && req.Password == request.NewPassword
                )))
                .ReturnsAsync(true);

            ArrangeFailureServiceResult(ExpectedErrorMessage);

            // Act
            var result = await _passwordService.UpdatePassword(UserId, request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(UserId);
            VerifyCallsToParameterService(3);

            _passwordHistoryServiceMock.Verify(f => f.FindPasswordHash(It.IsAny<SearchPasswordHistoryRequest>()), Times.Once());
        }

        /// <summary>
        ///     Tests the <see cref="PasswordService.UpdatePassword"/> method under successful conditions.
        ///     Ensures that when valid inputs are provided, the method returns a successful operation result.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task UpdatePassword_SuccessfulConditions_ReturnsGeneralOperationSuccess()
        {
            // Arrange 
            const string UserId = "id-123";

            var request = ArrangeUpdatePasswordRequest();
            var user = ArrangeMockUser(UserId, request.CurrentPassword);

            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = true });

            ArrangeUserLookupServiceMock(user, UserId, "");

            _userManagerMock
                .Setup(c => c.CheckPasswordAsync(user, request.CurrentPassword))
                .ReturnsAsync(true);
            _passwordHistoryServiceMock
                .Setup(f => f.FindPasswordHash(It.Is<SearchPasswordHistoryRequest>(
                    req => req.UserId == UserId && req.Password == request.NewPassword
                )))
                .ReturnsAsync(false);
            _userManagerMock
                .Setup(c => c.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            ArrangeSuccessServiceResult();

            // Act
            var result = await _passwordService.UpdatePassword(UserId, request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            VerifyCallsToLookupService(UserId);
            VerifyCallsToParameterService(3);

            _userManagerMock.Verify(c => c.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword), Times.Once);
        }

        private static User ArrangeMockUser(string userId, string password)
        {
            var passwordHasher = new PasswordHasher<User>();
            var passwordHashed = passwordHasher.HashPassword(null, password);
            return new User { Id = userId, UserName = "user123", PasswordHash = passwordHashed };
        }

        private static SetPasswordRequest ArrangeSetPasswordRequest()
        {
            return new SetPasswordRequest
            {
                Password = "Password123",
                PasswordConfirmed = "Password123"
            };
        }

        private static UpdatePasswordRequest ArrangeUpdatePasswordRequest()
        {
            return new UpdatePasswordRequest
            {
                CurrentPassword = "current password",
                NewPassword = "new password"
            };
        }

        private void ArrangeFailureServiceResult(string expectedErrorMessage)
        {
            var result = new ServiceResult
            {
                Success = false,
                Errors = new List<string> { expectedErrorMessage }
            };

            _serviceResultFactoryMock
                .Setup(x => x.GeneralOperationFailure(new[] { expectedErrorMessage }))
                .Returns(result);
        }

        private void ArrangeSuccessServiceResult()
        {
            _serviceResultFactoryMock
                .Setup(x => x.GeneralOperationSuccess())
                .Returns(new ServiceResult { Success = true });
        }

        private void ArrangeUserLookupServiceMock(User user, string userId, string expectedErrorMessage)
        {
            _userLookupServiceMock
                .Setup(u => u.FindUserById(userId))
                .ReturnsAsync(user == null
                    ? new UserLookupServiceResult
                    {
                        Success = false,
                        Errors = new[] { expectedErrorMessage }.ToList()
                    }
                    : new UserLookupServiceResult
                    {
                        Success = true,
                        UserFound = user
                    });
        }

        private void VerifyCallsToParameterService(int numberOfTimes)
        {
            _parameterValidatorMock.Verify(v => v.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(numberOfTimes));
        }

        private void VerifyCallsToLookupService(string id)
        {
            _userLookupServiceMock.Verify(l => l.FindUserById(id), Times.Once);
        }
    }
}
