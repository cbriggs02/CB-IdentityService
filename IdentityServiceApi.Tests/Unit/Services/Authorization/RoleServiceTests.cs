using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.ServiceResultModels.UserManagement;
using IdentityServiceApi.Models.ServiceResultModels.Shared;
using IdentityServiceApi.Services.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace IdentityServiceApi.Tests.Unit.Services.Authorization
{
    /// <summary>
    ///     Unit tests for the <see cref="RoleService"/> class.
    ///     This class contains test cases for various role scenarios, verifying the 
    ///     behavior of the role functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class RoleServiceTests
    {
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
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
        private readonly Mock<ILogger<RoleManager<IdentityRole>>> _roleManagerLoggerMock;
        private readonly Mock<IRoleStore<IdentityRole>> _roleStoreMock;
        private readonly List<IRoleValidator<IdentityRole>> _roleValidatorsMock;
        private readonly RoleService _roleService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RoleServiceTests"/> class.
        ///     This constructor sets up the mocked dependencies and creates an instance 
        ///     of the <see cref="RoleService"/> for testing.
        /// </summary>
        public RoleServiceTests()
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

            _roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleValidatorsMock = new List<IRoleValidator<IdentityRole>> { new RoleValidator<IdentityRole>() };
            _roleManagerLoggerMock = new Mock<ILogger<RoleManager<IdentityRole>>>();

            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                _roleStoreMock.Object,
                _roleValidatorsMock,
                _keyNormalizerMock.Object,
                _errorsMock.Object,
                _roleManagerLoggerMock.Object
            );

            _parameterValidatorMock = new Mock<IParameterValidator>();
            _serviceResultFactoryMock = new Mock<IServiceResultFactory>();
            _userLookupServiceMock = new Mock<IUserLookupService>();

            _roleService = new RoleService(_roleManagerMock.Object, _userManagerMock.Object, _parameterValidatorMock.Object, _serviceResultFactoryMock.Object, _userLookupServiceMock.Object);
        }

        /// <summary>
        ///     Tests that an <see cref="ArgumentNullException"/> is thrown when <see cref="RoleService"/> is 
        ///     instantiated with a null dependencies.
        /// </summary>
        [Fact]
        public void RoleService_NullDependencies_ThrowsArgumentNullException()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new RoleService(null, null, null, null, null));
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.AssignRoleAsync"/> method throws an <see cref="ArgumentNullException"/>
        ///     when a null, empty or white space parameters are provided.
        ///     This ensures that the service correctly validates the role name and id parameter before attempting to assign a role.
        /// </summary>
        /// <param name="input">
        ///     Used to test for invalid data like ( null, empty or whitespace )
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task AssignRole_InvalidRoleNamesAndId_ThrowsArgumentNullException(string input)
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _roleService.AssignRoleAsync(input, input));

            VerifyCallsToParameterService(1);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.AssignRoleAsync"/> returns a <see cref="ErrorMessages.User.NotFound"/> when 
        ///     providing a invalid user id to the method.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task AssignRole_NonExistentUserId_ReturnsNotFoundResult()
        {
            // Arrange 
            const string userId = "non-existent-id";
            const string roleName = Roles.User;
            const string expectedErrorMessage = ErrorMessages.User.NotFound;

            ArrangeUserLookupServiceMock(null, userId, expectedErrorMessage);
            ArrangeFailureServiceResult(expectedErrorMessage);

            // Act
            var result = await _roleService.AssignRoleAsync(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(userId);
            VerifyCallsToParameterService(2);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.AssignRoleAsync"/> returns a <see cref="ErrorMessages.Role.InactiveUser"/> when 
        ///     assigning a role to a user who is not activated in the system.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task AssignRole_UserNotActivated_ReturnsInactiveUserError()
        {
            // Arrange 
            const string userId = "existing-id";
            const string roleName = Roles.User;
            const string expectedErrorMessage = ErrorMessages.Role.InactiveUser;

            var user = CreateMockUser(false);

            ArrangeUserLookupServiceMock(user, userId, "");
            ArrangeFailureServiceResult(expectedErrorMessage);

            // Act
            var result = await _roleService.AssignRoleAsync(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(userId);
            VerifyCallsToParameterService(2);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.AssignRoleAsync"/> returns a <see cref="ErrorMessages.Role.InvalidRole"/> when 
        ///     providing a role that does not exist in the system.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task AssignRole_NonExistentRole_ReturnsInvalidRoleError()
        {
            // Arrange 
            const string userId = "existing-id";
            const string roleName = "non-existent-role";
            const string expectedErrorMessage = ErrorMessages.Role.InvalidRole;

            var user = CreateMockUser(true);

            ArrangeUserLookupServiceMock(user, userId, "");
            ArrangeFailureServiceResult(expectedErrorMessage);

            // Act
            var result = await _roleService.AssignRoleAsync(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(userId);
            VerifyCallsToParameterService(2);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.AssignRoleAsync"/> returns a <see cref="ErrorMessages.Role.HasRole"/> when 
        ///     assigning a role to a user whom already has that role.
        /// </summary>
        /// <param name="roleName">
        ///     Used to hold data for all roles a user could have.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Theory]
        [InlineData(Roles.SuperAdmin)]
        [InlineData(Roles.Admin)]
        [InlineData(Roles.User)]
        public async Task AssignRole_UserAlreadyHasRole_ReturnsAlreadyHasRoleError(string roleName)
        {
            // Arrange 
            const string userId = "existing-id";
            const string expectedErrorMessage = ErrorMessages.Role.HasRole;

            var user = CreateMockUser(true);

            ArrangeUserLookupServiceMock(user, userId, "");

            _roleManagerMock
                .Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _userManagerMock
                .Setup(u => u.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { roleName });

            ArrangeFailureServiceResult(expectedErrorMessage);

            // Act
            var result = await _roleService.AssignRoleAsync(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(userId);
            VerifyCallsToParameterService(2);

            _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        ///     Tests the behavior of the <see cref="RoleService.AssignRoleAsync"/> method when the role assignment operation fails.
        ///     It verifies that the operation returns a failure result with the appropriate error message when the user cannot be assigned to a role.
        /// </summary>
        /// <param name="roleName">
        ///     The role to be assigned to the user.
        ///     This can be any valid role name such as <see cref="Roles.SuperAdmin"/>, <see cref="Roles.Admin"/>, or <see cref="Roles.User"/>.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation. The result will indicate whether the operation was successful or not.
        /// </returns>
        [Theory]
        [InlineData(Roles.SuperAdmin)]
        [InlineData(Roles.Admin)]
        [InlineData(Roles.User)]
        public async Task AssignRole_AddToRoleAsyncFails_ReturnsGeneralOperationFailureResult(string roleName)
        {
            // Arrange 
            const string expectedErrorMessage = "Failed to add role to user.";
            const string userId = "existing-id";

            var user = CreateMockUser(true);

            ArrangeUserLookupServiceMock(user, userId, "");

            _roleManagerMock
                .Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _userManagerMock
                .Setup(u => u.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());
            _userManagerMock
                .Setup(a => a.AddToRoleAsync(user, roleName))
               .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = expectedErrorMessage }));

            ArrangeFailureServiceResult(expectedErrorMessage);

            // Act
            var result = await _roleService.AssignRoleAsync(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            VerifyCallsToParameterService(2);
            VerifyCallsToLookupService(userId);

            _userManagerMock.Verify(a => a.AddToRoleAsync(user, roleName), Times.Once);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.AssignRoleAsync"/> returns success when 
        ///     assigning a role to a user.
        /// </summary>
        /// <param name="roleName">
        ///     Used to hold data for all roles a user could be assigned.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Theory]
        [InlineData(Roles.SuperAdmin)]
        [InlineData(Roles.Admin)]
        [InlineData(Roles.User)]
        public async Task AssignRole_ValidCase_ReturnsSuccess(string roleName)
        {
            // Arrange 
            const string userId = "existing-id";

            var user = CreateMockUser(true);

            ArrangeUserLookupServiceMock(user, userId, "");

            _roleManagerMock
                .Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _userManagerMock
                .Setup(u => u.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());
            _userManagerMock
                .Setup(a => a.AddToRoleAsync(user, roleName))
                .ReturnsAsync(IdentityResult.Success);

            ArrangeSuccessServiceResult();

            // Act
            var result = await _roleService.AssignRoleAsync(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            VerifyCallsToParameterService(2);
            VerifyCallsToLookupService(userId);

            _userManagerMock.Verify(a => a.AddToRoleAsync(user, roleName), Times.Once);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.RemoveRoleAsync"/> method throws an <see cref="ArgumentNullException"/>
        ///     when a null, empty or white space parameters role are provided.
        ///     This verifies that the service correctly validates the role name and id parameter before attempting to remove a role.
        /// </summary>
        /// <param name="input">
        ///     Used to test for invalid data like ( null, empty or whitespace )
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task RemoveRole_InvalidRoleNameAndId_ThrowsArgumentNullException(string input)
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _roleService.RemoveRoleAsync(input, input));

            VerifyCallsToParameterService(1);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.RemoveRoleAsync"/> returns a <see cref="ErrorMessages.User.NotFound"/> when 
        ///     providing a invalid user id to the method.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task RemoveRole_NonExistentUserId_ReturnsNotFoundResult()
        {
            // Arrange 
            const string userId = "non-existent-id";
            const string roleName = Roles.User;
            const string expectedErrorMessage = ErrorMessages.User.NotFound;

            ArrangeUserLookupServiceMock(null, userId, expectedErrorMessage);
            ArrangeFailureServiceResult(expectedErrorMessage);

            // Act
            var result = await _roleService.RemoveRoleAsync(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(userId);
            VerifyCallsToParameterService(2);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.RemoveRoleAsync"/> returns a <see cref="ErrorMessages.Role.InvalidRole"/> when 
        ///     providing a role that does not exist in the system.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task RemoveRole_NonExistentRole_ReturnsInvalidRoleError()
        {
            // Arrange 
            const string userId = "existing-id";
            const string roleName = "non-existent-role";
            const string expectedErrorMessage = ErrorMessages.Role.InvalidRole;

            var user = CreateMockUser(true);

            ArrangeUserLookupServiceMock(user, userId, "");
            ArrangeFailureServiceResult(expectedErrorMessage);

            // Act
            var result = await _roleService.RemoveRoleAsync(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(userId);
            VerifyCallsToParameterService(2);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.RemoveRoleAsync"/> returns a <see cref="ErrorMessages.Role.MissingRole"/> when 
        ///     removing a role from a user who is not assigned that role.
        /// </summary>
        /// <param name="roleName">
        ///     Used to hold data for all roles that could be removed.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Theory]
        [InlineData(Roles.SuperAdmin)]
        [InlineData(Roles.Admin)]
        [InlineData(Roles.User)]
        public async Task RemoveRole_NonAssignedRole_ReturnsAlreadyMissingRoleError(string roleName)
        {
            // Arrange 
            const string userId = "existing-id";
            const string expectedErrorMessage = ErrorMessages.Role.MissingRole;

            var user = CreateMockUser(true);

            ArrangeUserLookupServiceMock(user, userId, "");

            _roleManagerMock
                .Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _userManagerMock
                .Setup(u => u.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            ArrangeFailureServiceResult(expectedErrorMessage);

            // Act
            var result = await _roleService.RemoveRoleAsync(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(userId);
            VerifyCallsToParameterService(2);

            _userManagerMock.Verify(u => u.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        ///     Tests the behavior of the <see cref="RoleService.RemoveRoleAsync"/> method when the role removal operation fails.
        ///     It verifies that the operation returns a failure result with the appropriate error message when the user cannot be removed from the role.
        /// </summary>
        /// <param name="roleName">
        ///     The role to be removed from the user.
        ///     This can be any valid role name such as <see cref="Roles.SuperAdmin"/>, <see cref="Roles.Admin"/>, or <see cref="Roles.User"/>.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation. The result will indicate whether the operation was successful or not.
        /// </returns>
        [Theory]
        [InlineData(Roles.SuperAdmin)]
        [InlineData(Roles.Admin)]
        [InlineData(Roles.User)]
        public async Task RemoveRole_RemoveFromRoleAsyncFails_ReturnsGeneralOperationFailureResult(string roleName)
        {
            // Arrange 
            const string expectedErrorMessage = "Failed to remove role from user.";
            const string userId = "existing-id";

            var user = CreateMockUser(true);

            ArrangeUserLookupServiceMock(user, userId, "");

            _roleManagerMock
                .Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _userManagerMock
                .Setup(u => u.GetRolesAsync(user))
                .ReturnsAsync(new List<string>() { roleName });
            _userManagerMock
                .Setup(r => r.RemoveFromRoleAsync(user, roleName))
               .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = expectedErrorMessage }));

            ArrangeFailureServiceResult(expectedErrorMessage);

            // Act
            var result = await _roleService.RemoveRoleAsync(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            VerifyCallsToParameterService(2);
            VerifyCallsToLookupService(userId);

            _userManagerMock.Verify(r => r.RemoveFromRoleAsync(user, roleName), Times.Once);
        }

        /// <summary>
        ///     Tests the behavior of the <see cref="RoleService.RemoveRoleAsync"/> method when the role removal operation succeeds.
        ///     It verifies that the operation returns a success result when the user is successfully removed from a role.
        /// </summary>
        /// <param name="roleName">
        ///     The role to be removed from the user.
        ///     This can be any valid role name such as <see cref="Roles.SuperAdmin"/>, <see cref="Roles.Admin"/>, or <see cref="Roles.User"/>.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation. The result will indicate whether the operation was successful or not.
        /// </returns>
        [Theory]
        [InlineData(Roles.SuperAdmin)]
        [InlineData(Roles.Admin)]
        [InlineData(Roles.User)]
        public async Task RemoveRole_RemoveFromRoleAsyncSucceeds_ReturnsGeneralOperationSuccessResult(string roleName)
        {
            // Arrange 
            const string userId = "existing-id";

            var user = CreateMockUser(true);

            ArrangeUserLookupServiceMock(user, userId, "");

            _roleManagerMock
                .Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _userManagerMock
                .Setup(u => u.GetRolesAsync(user))
                .ReturnsAsync(new List<string>() { roleName });
            _userManagerMock
                .Setup(r => r.RemoveFromRoleAsync(user, roleName))
               .ReturnsAsync(IdentityResult.Success);

            ArrangeSuccessServiceResult();

            // Act
            var result = await _roleService.RemoveRoleAsync(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            VerifyCallsToParameterService(2);
            VerifyCallsToLookupService(userId);

            _userManagerMock.Verify(r => r.RemoveFromRoleAsync(user, roleName), Times.Once);
        }

        private void ArrangeUserLookupServiceMock(User user, string userId, string expectedErrorMessage)
        {
            _userLookupServiceMock
                .Setup(u => u.FindUserByIdAsync(userId))
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

        private static User CreateMockUser(bool accountStatus)
        {
            const string mockUserName = "test-user";
            return new User { UserName = mockUserName, AccountStatus = accountStatus ? 1 : 0 };
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

        private void VerifyCallsToLookupService(string id)
        {
            _userLookupServiceMock.Verify(l => l.FindUserByIdAsync(id), Times.Once);
        }

        private void VerifyCallsToParameterService(int numberOfTimes)
        {
            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(numberOfTimes));
        }
    }
}
