using AutoMapper;
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

            VerifyCallsToParameterValidatorForString();
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
        public async Task GetUser_UserTryingToGetOtherUser_ReturnsForbiddenFailureResult(string roleName)
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

            VerifyCallsToParameterValidatorForString();
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
        public async Task GetUser_NonExistentUserId_ReturnsNotFoundFailureResult()
        {
            // Arrange 
            const string UserId = "non-existent-id";
            const string ExpectedErrorMessage = ErrorMessages.User.NotFound;

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
            VerifyCallsToParameterValidatorForString();
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
            VerifyCallsToParameterValidatorForString();
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.CreateUser"/> method 
        ///     throws an <see cref="ArgumentNullException"/> when the provided 
        ///     <paramref name="user"/> parameter is null.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task CreateUser_NullRequestParameterObject_ThrowsArgumentNullException()
        {
            // Arrange 
            _parameterValidatorMock
                .Setup(x => x.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.CreateUser(null));

            VerifyCallsToParameterValidatorForObject();
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.CreateUser"/> method 
        ///     throws an <see cref="ArgumentNullException"/> when one or more 
        ///     required user properties contain a null or empty value.
        /// </summary>
        /// <param name="input">
        ///     The invalid input value used to test user properties. 
        ///     This can be null, an empty string, or a whitespace string.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task CreateUser_InvalidUserProperties_ThrowsArgumentNullException(string input)
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            var user = new UserDTO
            {
                UserName = input,
                FirstName = input,
                LastName = input,
                Email = input,
                PhoneNumber = input,
                Country = input
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.CreateUser(user));

            VerifyCallsToParameterValidatorForString();
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.CreateUser"/> method 
        ///     returns a failure result when the user creation process fails.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task CreateUser_CreateAsyncFails_ReturnsUserOperationFailureResult()
        {
            // Arrange
            const string ExpectedErrorMessage = "User creation failed";

            var user = ArrangeMockUserDTO();

            _userManagerMock
                .Setup(c => c.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = ExpectedErrorMessage }));

            ArrangeUserOperationFailureResult(ExpectedErrorMessage);

            // Act
            var result = await _userService.CreateUser(user);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToParameterValidatorForObject();

            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(6));
            _userManagerMock.Verify(c => c.CreateAsync(It.IsAny<User>()), Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.CreateUser"/> method 
        ///     returns a success result when the user creation process succeeds.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task CreateUser_CreateAsyncSucceeds_ReturnsUserOperationSuccessResult()
        {
            // Arrange
            var user = ArrangeMockUserDTO();

            _userManagerMock
                .Setup(c => c.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            _userServiceResultFactoryMock
                .Setup(f => f.UserOperationSuccess(It.IsAny<UserDTO>()))
                .Returns((UserDTO u) => new UserServiceResult { Success = true, User = u });

            // Act
            var result = await _userService.CreateUser(user);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.User);
            Assert.True(result.Success);

            VerifyCallsToParameterValidatorForObject();

            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(6));
            _userManagerMock.Verify(c => c.CreateAsync(It.IsAny<User>()), Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.UpdateUser"/> method 
        ///     throws an <see cref="ArgumentNullException"/> when provided 
        ///     with an invalid user ID.
        /// </summary>
        /// <param name="input">
        ///     The invalid user ID to test, which may be null, empty, or whitespace.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task UpdateUser_InvalidId_ThrowsArgumentNullException(string input)
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            var user = ArrangeMockUserDTO();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.UpdateUser(input, user));

            VerifyCallsToParameterValidatorForString();
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.UpdateUser"/> method 
        ///     throws an <see cref="ArgumentNullException"/> when the user object is null.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task UpdateUser_NullUserObject_ThrowsArgumentNullException()
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.UpdateUser("id-123", null));

            VerifyCallsToParameterValidatorForObject();
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.UpdateUser"/> method 
        ///     throws an <see cref="ArgumentNullException"/> when the user object 
        ///     contains invalid properties (e.g., null, empty, or whitespace values).
        /// </summary>
        /// <param name="input">
        ///     The invalid value used for user properties, which may be null, empty, or whitespace.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task UpdateUser_InvalidUserProperties_ThrowsArgumentNullException(string input)
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            var user = new UserDTO
            {
                UserName = input,
                FirstName = input,
                LastName = input,
                Email = input,
                PhoneNumber = input,
                Country = input
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.UpdateUser("id-123", user));

            VerifyCallsToParameterValidatorForString();
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.UpdateUser"/> method 
        ///     returns a failure result with a forbidden error when a user 
        ///     attempts to update another user's information without proper authorization.
        /// </summary>
        /// <param name="roleName">
        ///     The role of the user attempting to perform the update.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Theory]
        [InlineData(Roles.Admin)]
        [InlineData(Roles.User)]
        public async Task UpdateUser_UserTryingToUpdateOtherUser_ReturnsForbiddenFailureResult(string roleName)
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

            ArrangeGeneralOperationFailureServiceResult(ExpectedErrorMessage);

            var userDTO = ArrangeMockUserDTO();

            // Act
            var result = await _userService.UpdateUser(UserId, userDTO);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(7));
            _permissionServiceMock.Verify(p => p.ValidatePermissions(UserId), Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.UpdateUser"/> method 
        ///     returns a failure result with a not found error when attempting 
        ///     to update a user that does not exist.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task UpdateUser_NonExistentUserId_ReturnsNotFoundFailureResult()
        {
            // Arrange 
            const string UserId = "non-existent-id";
            const string ExpectedErrorMessage = ErrorMessages.User.NotFound;

            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = true });

            ArrangeUserLookupServiceMock(null, UserId, ExpectedErrorMessage);
            ArrangeGeneralOperationFailureServiceResult(ExpectedErrorMessage);

            var userDTO = ArrangeMockUserDTO();

            // Act
            var result = await _userService.UpdateUser(UserId, userDTO);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(UserId);

            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(7));
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.UpdateUser"/> method 
        ///     returns a failure result when <see cref="UserManager{TUser}.UpdateAsync"/> fails.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task UpdateUser_UpdateAsyncFails_ReturnsOperationFailureResult()
        {
            // Arrange 
            const string ExpectedErrorMessage = "User update failed";
            const string UserId = "id-123";

            var user = ArrangeMockUser(UserId);

            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = true });

            ArrangeUserLookupServiceMock(user, UserId, "");

            _userManagerMock
                .Setup(u => u.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = ExpectedErrorMessage }));

            ArrangeGeneralOperationFailureServiceResult(ExpectedErrorMessage);

            var userDTO = ArrangeMockUserDTO();

            // Act
            var result = await _userService.UpdateUser(UserId, userDTO);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(UserId);
            _userManagerMock.Verify(u => u.UpdateAsync(It.IsAny<User>()), Times.Once);
            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(7));
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.UpdateUser"/> method 
        ///     successfully updates an existing user and returns a success result.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task UpdateUser_UserFound_ReturnsSuccessOperationResult()
        {
            // Arrange 
            const string UserId = "id-123";

            var user = ArrangeMockUser(UserId);

            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = true });

            ArrangeUserLookupServiceMock(user, UserId, "");

            _userManagerMock
                .Setup(u => u.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            ArrangeGeneralOperationSuccessServiceResult();

            var userDTO = ArrangeMockUserDTO();

            // Act
            var result = await _userService.UpdateUser(UserId, userDTO);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            VerifyCallsToLookupService(UserId);
            _userManagerMock.Verify(u => u.UpdateAsync(It.IsAny<User>()), Times.Once);
            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(7));
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.DeleteUser"/> method 
        ///     throws an <see cref="ArgumentNullException"/> when an invalid 
        ///     user ID (null, empty, or whitespace) is provided.
        /// </summary>
        /// <param name="input">
        ///     The invalid user ID to be tested.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task DeleteUser_InvalidId_ThrowsArgumentNullException(string input)
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.DeleteUser(input));

            VerifyCallsToParameterValidatorForString();
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.DeleteUser"/> method 
        ///     returns a forbidden failure result when a user attempts to delete 
        ///     another user without the necessary permissions.
        /// </summary>
        /// <param name="roleName">
        ///     The role of the user attempting to delete another user.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Theory]
        [InlineData(Roles.Admin)]
        [InlineData(Roles.User)]
        public async Task DeleteUser_UserTryingToDeleteOtherUser_ReturnsForbiddenFailureResult(string roleName)
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

            ArrangeGeneralOperationFailureServiceResult(ExpectedErrorMessage);

            // Act
            var result = await _userService.DeleteUser(UserId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToParameterValidatorForString();
            _permissionServiceMock.Verify(p => p.ValidatePermissions(UserId), Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.DeleteUser"/> method 
        ///     returns a not found failure result when attempting to delete a 
        ///     user that does not exist.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task DeleteUser_NonExistentUserId_ReturnsNotFoundFailureResult()
        {
            // Arrange 
            const string UserId = "non-existent-id";
            const string ExpectedErrorMessage = ErrorMessages.User.NotFound;

            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = true });

            ArrangeUserLookupServiceMock(null, UserId, ExpectedErrorMessage);
            ArrangeUserOperationFailureResult(ExpectedErrorMessage);

            // Act
            var result = await _userService.DeleteUser(UserId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(UserId);
            VerifyCallsToParameterValidatorForString();
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.DeleteUser"/> method 
        ///     returns an operation failure result when the user deletion process fails.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task DeleteUser_DeleteAsyncFails_ReturnsOperationFailureResult()
        {
            // Arrange 
            const string ExpectedErrorMessage = "User deletion failed";
            const string UserId = "id-123";

            var user = ArrangeMockUser(UserId);

            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = true });

            ArrangeUserLookupServiceMock(user, UserId, "");

            _userManagerMock
                .Setup(d => d.DeleteAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = ExpectedErrorMessage }));

            ArrangeGeneralOperationFailureServiceResult(ExpectedErrorMessage);

            // Act
            var result = await _userService.DeleteUser(UserId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(UserId);
            VerifyCallsToParameterValidatorForString();
            _userManagerMock.Verify(d => d.DeleteAsync(It.IsAny<User>()), Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.DeleteUser"/> method 
        ///     successfully deletes an existing user and returns a success operation result.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task DeleteUser_UserFound_ReturnsSuccessOperationResult()
        {
            // Arrange 
            const string UserId = "id-123";

            var user = ArrangeMockUser(UserId);

            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = true });

            ArrangeUserLookupServiceMock(user, UserId, "");

            _userManagerMock
                .Setup(d => d.DeleteAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            ArrangeGeneralOperationSuccessServiceResult();

            // Act
            var result = await _userService.DeleteUser(UserId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            VerifyCallsToLookupService(UserId);
            VerifyCallsToParameterValidatorForString();

            _userHistoryCleanupServiceMock.Verify(d => d.DeletePasswordHistory(UserId), Times.Once);
            _userManagerMock.Verify(d => d.DeleteAsync(It.IsAny<User>()), Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.ActivateUser"/> method throws 
        ///     an <see cref="ArgumentNullException"/> when an invalid user ID (null, 
        ///     empty, or whitespace) is provided.
        /// </summary>
        /// <param name="input">
        ///     The user ID input that will be tested for invalid values.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ActivateUser_InvalidId_ThrowsArgumentNullException(string input)
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.ActivateUser(input));

            VerifyCallsToParameterValidatorForString();
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.ActivateUser"/> method returns a forbidden result 
        ///     when a user attempts to activate another user, an admin, or a super admin. Admins are also 
        ///     restricted from activating other admins or super admins.
        /// </summary>
        /// <param name="roleName">
        ///     The role of the user attempting to activate another user. This test is focused on ensuring that users 
        ///     with insufficient privileges (e.g., standard users or admins) cannot activate other users or users 
        ///     with higher privileges.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Theory]
        [InlineData(Roles.User)]
        [InlineData(Roles.Admin)]
        public async Task ActivateUser_UserTryingToActivateOtherUser_ReturnsForbiddenFailureResult(string roleName)
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

            ArrangeGeneralOperationFailureServiceResult(ExpectedErrorMessage);

            // Act
            var result = await _userService.ActivateUser(UserId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToParameterValidatorForString();
            _permissionServiceMock.Verify(p => p.ValidatePermissions(UserId), Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.ActivateUser"/> method returns a forbidden result when a user 
        ///     attempts to activate their own account with insufficient privileges. This test ensures that users cannot 
        ///     activate their own account if they don't have the required permissions.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation of the unit test.
        /// </returns>
        [Fact]
        public async Task ActivateUser_UserTryingToActivateItselfWithInsufficientPrivileges_ReturnsForbiddenFailureResult()
        {
            // Arrange
            const string ExpectedErrorMessage = ErrorMessages.Authorization.Forbidden;
            const string UserId = "id-123";

            var user = ArrangeMockUser(UserId);

            _userManagerMock
                .Setup(a => a.AddToRoleAsync(user, Roles.User))
                .ReturnsAsync(IdentityResult.Success);
            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = false, Errors = new List<string> { ExpectedErrorMessage } });

            ArrangeGeneralOperationFailureServiceResult(ExpectedErrorMessage);

            // Act
            var result = await _userService.ActivateUser(UserId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToParameterValidatorForString();
            _permissionServiceMock.Verify(p => p.ValidatePermissions(UserId), Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.ActivateUser"/> method 
        ///     returns a not found failure result when the user does not exist.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task ActivateUser_NonExistentUserId_ReturnsNotFoundFailureResult()
        {
            // Arrange 
            const string UserId = "non-existent-id";
            const string ExpectedErrorMessage = ErrorMessages.User.NotFound;

            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = true });

            ArrangeUserLookupServiceMock(null, UserId, ExpectedErrorMessage);
            ArrangeUserOperationFailureResult(ExpectedErrorMessage);

            // Act
            var result = await _userService.ActivateUser(UserId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(UserId);
            VerifyCallsToParameterValidatorForString();
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.ActivateUser"/> method 
        ///     returns an operation failure result when the user is already activated.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task ActivateUser_UserAlreadyActivated_ReturnsOperationFailureResult()
        {
            // Arrange 
            const string ExpectedErrorMessage = ErrorMessages.User.AlreadyActivated;
            const string UserId = "id-123";

            var user = new User { Id = UserId, UserName = "user123", AccountStatus = 1 };

            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = true });

            ArrangeUserLookupServiceMock(user, UserId, "");
            ArrangeGeneralOperationFailureServiceResult(ExpectedErrorMessage);

            // Act
            var result = await _userService.ActivateUser(UserId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(UserId);
            VerifyCallsToParameterValidatorForString();
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.ActivateUser"/> method 
        ///     returns an operation failure result when <see cref="UserManager{TUser}.UpdateAsync"/> 
        ///     fails to update the user.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task ActivateUser_UpdateAsyncFails_ReturnsOperationFailureResult()
        {
            // Arrange 
            const string ExpectedErrorMessage = "User update failed";
            const string UserId = "id-123";

            var user = ArrangeMockUser(UserId);

            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = true });

            ArrangeUserLookupServiceMock(user, UserId, "");

            _userManagerMock
                .Setup(u => u.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = ExpectedErrorMessage }));

            ArrangeGeneralOperationFailureServiceResult(ExpectedErrorMessage);

            // Act
            var result = await _userService.ActivateUser(UserId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(ExpectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(UserId);
            VerifyCallsToParameterValidatorForString();

            _userManagerMock.Verify(u => u.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="UserService.ActivateUser"/> method 
        ///     successfully activates a user who is found and not already activated, 
        ///     returning a success operation result.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task ActivateUser_UserFoundAndNotActivated_ReturnsSuccessOperationResult()
        {
            // Arrange 
            const string UserId = "id-123";

            var user = ArrangeMockUser(UserId);

            _permissionServiceMock
                .Setup(p => p.ValidatePermissions(UserId))
                .ReturnsAsync(new ServiceResult { Success = true });

            ArrangeUserLookupServiceMock(user, UserId, "");

            _userManagerMock
                .Setup(u => u.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            ArrangeGeneralOperationSuccessServiceResult();

            // Act
            var result = await _userService.ActivateUser(UserId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            VerifyCallsToLookupService(UserId);
            VerifyCallsToParameterValidatorForString();

            _userManagerMock.Verify(u => u.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        private static UserDTO ArrangeMockUserDTO()
        {
            return new UserDTO
            {
                UserName = "user123",
                FirstName = "Joe",
                LastName = "Doe",
                Email = "doe@Gmail.com",
                PhoneNumber = "613-123-1234",
                Country = "Canada"
            };
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

        private void ArrangeGeneralOperationFailureServiceResult(string expectedErrorMessage)
        {
            var result = new ServiceResult
            {
                Success = false,
                Errors = new List<string> { expectedErrorMessage }
            };

            _userServiceResultFactoryMock
                .Setup(x => x.GeneralOperationFailure(new[] { expectedErrorMessage }))
                .Returns(result);
        }

        private void ArrangeGeneralOperationSuccessServiceResult()
        {
            var result = new ServiceResult
            {
                Success = true,
            };

            _userServiceResultFactoryMock
                .Setup(x => x.GeneralOperationSuccess())
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

        private void VerifyCallsToParameterValidatorForString()
        {
            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        private void VerifyCallsToParameterValidatorForObject()
        {
            _parameterValidatorMock.Verify(v => v.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }

        private void VerifyCallsToLookupService(string id)
        {
            _userLookupServiceMock.Verify(l => l.FindUserById(id), Times.Once);
        }
    }
}
