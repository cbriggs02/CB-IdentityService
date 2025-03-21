﻿using AutoMapper;
using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Authorization;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.Entities;
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
    ///     Unit tests for the <see cref="UserService"/> class.
    ///     This class contains test cases for various user scenarios, verifying the 
    ///     behavior of the user functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class UserServiceTests
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
        private readonly Mock<IUserServiceResultFactory> _userServiceResultFactoryMock;
        private readonly Mock<IPasswordHistoryCleanupService> _userHistoryCleanupServiceMock;
        private readonly Mock<IPermissionService> _permissionServiceMock;
        private readonly Mock<IParameterValidator> _parameterValidatorMock;
        private readonly Mock<IUserLookupService> _userLookupServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UserService _userService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserServiceTests"/> class.
        ///     This constructor sets up the mocked dependencies and creates an instance 
        ///     of the <see cref="UserService"/> for testing.
        /// </summary>
        public UserServiceTests()
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

            _userServiceResultFactoryMock = new Mock<IUserServiceResultFactory>();
            _userHistoryCleanupServiceMock = new Mock<IPasswordHistoryCleanupService>();
            _permissionServiceMock = new Mock<IPermissionService>();
            _parameterValidatorMock = new Mock<IParameterValidator>();
            _userLookupServiceMock = new Mock<IUserLookupService>();
            _mapperMock = new Mock<IMapper>();

            _userService = new UserService(_userManagerMock.Object, _userServiceResultFactoryMock.Object, _userHistoryCleanupServiceMock.Object, _permissionServiceMock.Object, _parameterValidatorMock.Object, _userLookupServiceMock.Object, _mapperMock.Object);
        }

        /// <summary>
        ///     Tests that an <see cref="ArgumentNullException"/> is thrown when <see cref="UserService"/> is 
        ///     instantiated with a null dependencies.
        /// </summary>
        [Fact]
        public void UserService_NullDependencies_ThrowsArgumentNullException()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UserService(null, null, null, null, null, null, null));
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.GetUser"/> method throws 
        ///     an <see cref="ArgumentNullException"/> when the provided user ID is null or empty.
        /// </summary>
        /// <param name="input">
        ///     The invalid user ID input to test, which may be null, empty, or whitespace.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetUser_InvalidId_ThrowsArgumentNullException(string input)
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.GetUser(input));

            VerifyCallsToParameterValidator();
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.GetUser"/> method returns 
        ///     a forbidden failure result when a user attempts to retrieve another user's data 
        ///     without sufficient permissions.
        /// </summary>
        /// <param name="roleName">
        ///     The role of the user attempting to access another user's data.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Theory]
        [InlineData(Roles.Admin)]
        [InlineData(Roles.User)]
        public async Task GetUser_TryingToGetOtherUser_ReturnsForbiddenFailureResult(string roleName)
        {
            // Arrange
            const string ExpectedErrorMessage = ErrorMessages.Authorization.Forbidden;
            const string UserId = "id-123";
            const string OtherUserId = "id-999";

            var user = ArrangeMockUser(OtherUserId);

            _userManagerMock
                .Setup(a => a.AddToRoleAsync(user, roleName))
                .ReturnsAsync(IdentityResult.Success);
            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = false, Errors = new List<string> { ExpectedErrorMessage } });

            ArrangeUserOperationFailureResult(ExpectedErrorMessage);

            // Act
            var result = await _userService.GetUser(UserId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToParameterValidator();
            _permissionServiceMock.Verify(p => p.ValidatePermissions(UserId), Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.GetUser"/> method returns 
        ///     a failure result when the specified user ID does not exist.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task GetUser_NonExistentUserId_ReturnsInvalidCredentialsFailureResult()
        {
            // Arrange 
            const string UserId = "non-existent-id";
            const string ExpectedErrorMessage = ErrorMessages.Password.InvalidCredentials;

            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = true });

            ArrangeUserLookupServiceMock(null, UserId, ExpectedErrorMessage);
            ArrangeUserOperationFailureResult(ExpectedErrorMessage);

            // Act
            var result = await _userService.GetUser(UserId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(UserId);
            VerifyCallsToParameterValidator();
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.GetUser"/> method returns 
        ///     a success result when the specified user is found.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task GetUser_UserFound_ReturnsUserOperationSuccessResult()
        {
            // Arrange 
            const string UserId = "user-id";
            var user = ArrangeMockUser(UserId);

            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = true });

            ArrangeUserLookupServiceMock(user, UserId, "");

            var userDTO = new UserDTO { UserName = user.UserName };
            _mapperMock.Setup(m => m.Map<UserDTO>(It.IsAny<User>())).Returns(userDTO);

            ArrangeUserOperationSuccessResult(userDTO);

            // Act
            var result = await _userService.GetUser(UserId);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.User);
            Assert.True(result.Success);

            VerifyCallsToLookupService(UserId);
            VerifyCallsToParameterValidator();
        }

        private static User ArrangeMockUser(string userId)
        {
            return new User { Id = userId, UserName = "user123" };
        }

        private void ArrangeUserOperationFailureResult(string expectedErrorMessage)
        {
            var result = new UserServiceResult
            {
                Success = false,
                Errors = new List<string> { expectedErrorMessage }
            };

            _userServiceResultFactoryMock
                .Setup(x => x.UserOperationFailure(new[] { expectedErrorMessage }))
                .Returns(result);
        }

        private void ArrangeUserOperationSuccessResult(UserDTO user)
        {
            var result = new UserServiceResult
            {
                Success = true,
                User = user
            };

            _userServiceResultFactoryMock
                .Setup(r => r.UserOperationSuccess(user))
                .Returns(result);
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

        private void VerifyCallsToParameterValidator()
        {
            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        private void VerifyCallsToLookupService(string id)
        {
            _userLookupServiceMock.Verify(l => l.FindUserById(id), Times.Once);
        }
    }
}
