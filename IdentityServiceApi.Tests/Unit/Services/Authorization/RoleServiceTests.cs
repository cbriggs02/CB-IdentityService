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
        ///     Tests that the <see cref="RoleService.CreateRole"/> method throws an <see cref="ArgumentNullException"/>
        ///     when a null role name is provided, or a empty string is provided.
        ///     This verifies that the service correctly validates input parameters before proceeding with role creation.
        /// </summary>
        /// <param name="roleName">
        ///     Used to test for invalid data like ( null, empty or whitespace )
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task CreateRole_NullAndEmptyRoleName_ThrowsArgumentNullException(string roleName)
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _roleService.CreateRole(roleName));

            VerifyCallsToParameterService(1);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.CreateRole"/> returns a error message 
        ///     <see cref="ErrorMessages.Role.AlreadyExist"/> when providing a already existing roles to the creation method.
        /// </summary>
        /// <param name="roleName">
        ///     Used to test cases for all existing roles.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous test operation.
        /// </returns>
        [Theory]
        [InlineData(Roles.Admin)]
        [InlineData(Roles.SuperAdmin)]
        [InlineData(Roles.User)]
        public async Task CreateRole_ExistentRole_ReturnsRoleAlreadyExistsError(string roleName)
        {
            // Arrange
            const string expectedErrorMessage = ErrorMessages.Role.AlreadyExist;

            _roleManagerMock
                .Setup(r => r.RoleExistsAsync(roleName))
                .ReturnsAsync(true);

            ArrangeFailureServiceResult(expectedErrorMessage);

            // Act
            var result = await _roleService.CreateRole(roleName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            _roleManagerMock.Verify(r => r.RoleExistsAsync(roleName), Times.Once);

            VerifyCallsToParameterService(1);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.CreateRole"/> returns a <see cref="ServiceResult"/>
        ///     with success set to true when providing role to the creation method that doesn't already exist.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous test operation.
        /// </returns>
        [Fact]
        public async Task CreateRole_NonExistentRole_ReturnsSuccessResult()
        {
            // Arrange
            const string roleName = "new-role";

            _roleManagerMock
                .Setup(r => r.RoleExistsAsync(roleName))
                .ReturnsAsync(false);
            _roleManagerMock
                .Setup(c => c.CreateAsync(It.Is<IdentityRole>(role => role.Name == roleName)))
                .ReturnsAsync(IdentityResult.Success);

            ArrangeSuccessServiceResult();

            // Act
            var result = await _roleService.CreateRole(roleName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            _roleManagerMock.Verify(c => c.CreateAsync(It.Is<IdentityRole>(role => role.Name == roleName)), Times.Once);

            VerifyCallsToParameterService(1);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.DeleteRole"/> method throws an <see cref="ArgumentNullException"/>
        ///     when a null role name is provided.
        ///     This ensures that the service correctly validates the role name parameter before attempting to delete a role.
        /// </summary>
        /// <param name="id">
        ///     Used to test for invalid data like ( null, empty or whitespace )
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task DeleteRole_NullAndEmptyId_ThrowsArgumentNullException(string id)
        {
            // Arrange
            _parameterValidatorMock
                .Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ArgumentNullException>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _roleService.DeleteRole(id));

            VerifyCallsToParameterService(1);
        }
        
        /// <summary>
        ///     Tests that the <see cref="RoleService.DeleteRole"/> returns a <see cref="ErrorMessages.Role.NotFound"/> 
        ///     when providing a invalid role id to the deletion method.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task DeleteRole_InvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            const string roleId = "invalid-id";
            const string expectedErrorMessage = ErrorMessages.Role.NotFound;

            _roleManagerMock
                .Setup(f => f.FindByIdAsync(roleId))
                .ReturnsAsync((IdentityRole)null);

            ArrangeFailureServiceResult(expectedErrorMessage);

            // Act
            var result = await _roleService.DeleteRole(roleId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            _roleManagerMock.Verify(f => f.FindByIdAsync(roleId), Times.Once);

            VerifyCallsToParameterService(1);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.DeleteRole"/> returns a <see cref="ServiceResult"/>
        ///     with success set to true when providing role id to the deletion method that already exist.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous test operation.
        /// </returns>
        [Fact]
        public async Task DeleteRole_ValidId_ReturnsSuccess()
        {
            // Arrange
            const string roleId = "valid-id";
            var role = new IdentityRole { Id = roleId };

            _roleManagerMock
                .Setup(f => f.FindByIdAsync(roleId))
                .ReturnsAsync(role);
            _roleManagerMock
                .Setup(c => c.DeleteAsync(It.Is<IdentityRole>(role => role.Id == roleId)))
                .ReturnsAsync(IdentityResult.Success);

            ArrangeSuccessServiceResult();

            // Act
            var result = await _roleService.DeleteRole(roleId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            _roleManagerMock.Verify(c => c.DeleteAsync(It.Is<IdentityRole>(role => role.Id == roleId)), Times.Once);

            VerifyCallsToParameterService(1);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.AssignRole"/> method throws an <see cref="ArgumentNullException"/>
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
            await Assert.ThrowsAsync<ArgumentNullException>(() => _roleService.AssignRole(input, input));

            VerifyCallsToParameterService(1);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.AssignRole"/> returns a <see cref="ErrorMessages.User.NotFound"/> when 
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
            var result = await _roleService.AssignRole(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(userId);
            VerifyCallsToParameterService(2);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.AssignRole"/> returns a <see cref="ErrorMessages.Role.InactiveUser"/> when 
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
            var result = await _roleService.AssignRole(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(userId);
            VerifyCallsToParameterService(2);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.AssignRole"/> returns a <see cref="ErrorMessages.Role.InvalidRole"/> when 
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
            var result = await _roleService.AssignRole(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(userId);
            VerifyCallsToParameterService(2);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.AssignRole"/> returns a <see cref="ErrorMessages.Role.HasRole"/> when 
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
            var result = await _roleService.AssignRole(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(userId);
            VerifyCallsToParameterService(2);

            _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.AssignRole"/> returns success when 
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
            var result = await _roleService.AssignRole(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            VerifyCallsToParameterService(2);
            VerifyCallsToLookupService(userId);

            _userManagerMock.Verify(a => a.AddToRoleAsync(user, roleName), Times.Once);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.RemoveRole"/> method throws an <see cref="ArgumentNullException"/>
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
            await Assert.ThrowsAsync<ArgumentNullException>(() => _roleService.RemoveRole(input, input));

            VerifyCallsToParameterService(1);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.RemoveRole"/> returns a <see cref="ErrorMessages.User.NotFound"/> when 
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
            var result = await _roleService.RemoveRole(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(userId);
            VerifyCallsToParameterService(2);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.RemoveRole"/> returns a <see cref="ErrorMessages.Role.InvalidRole"/> when 
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
            var result = await _roleService.RemoveRole(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(userId);
            VerifyCallsToParameterService(2);
        }

        /// <summary>
        ///     Tests that the <see cref="RoleService.RemoveRole"/> returns a <see cref="ErrorMessages.Role.MissingRole"/> when 
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
            var result = await _roleService.RemoveRole(userId, roleName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedErrorMessage, result.Errors);

            VerifyCallsToLookupService(userId);
            VerifyCallsToParameterService(2);

            _userManagerMock.Verify(u => u.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        ///     Sets up a mock for the user lookup service to simulate finding a user by their ID.
        /// </summary>
        /// <param name="user">
        ///     The user object to be returned if found; pass <c>null</c> if the user is not found.
        /// </param>
        /// <param name="userId">
        ///     The ID of the user to be searched for in the mock setup.
        /// </param>
        /// <param name="expectedErrorMessage">
        ///     The error message to include in the result if the user is not found.
        /// </param>
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

        /// <summary>
        ///     Creates a mock <see cref="User"/> object with the specified account status.
        /// </summary>
        /// <param name="accountStatus">
        ///     The account status to set for the mock user. This could indicate whether the user is active or inactive.
        /// </param>
        /// <returns>
        ///     A <see cref="User"/> object initialized with the specified account status and a predefined username.
        /// </returns>
        private static User CreateMockUser(bool accountStatus)
        {
            const string mockUserName = "test-user";
            return new User { UserName = mockUserName, AccountStatus = accountStatus ? 1 : 0 };
        }

        /// <summary>
        ///     Sets up the mock service factory result to return a 
        ///     <see cref="ServiceResult"/> indicating a failure with the specified error message.
        /// </summary>
        /// <param name="expectedErrorMessage">
        ///     The expected error message to be included in the 
        ///     <see cref="ServiceResult"/> indicating the reason for failure.
        /// </param>
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

        /// <summary>
        ///     Sets up the mock service factory result to return a 
        ///     <see cref="ServiceResult"/> indicating a success..
        /// </summary>
        private void ArrangeSuccessServiceResult()
        {
            _serviceResultFactoryMock
                .Setup(x => x.GeneralOperationSuccess())
                .Returns(new ServiceResult { Success = true });
        }

        /// <summary>
        ///     Verifies that the <see cref="_userLookupServiceMock"/> mock was called once
        ///     to find a user by the specified id.
        /// </summary>
        /// <param name="id">
        ///     The id string used as the parameter to look up the user in the test.
        /// </param>
        private void VerifyCallsToLookupService(string id)
        {
            _userLookupServiceMock.Verify(l => l.FindUserById(id), Times.Once);
        }

        /// <summary>
        ///     Verifies that the <see cref="_parameterValidatorMock"/> mock was called with 
        ///     expected validation methods during test execution.
        /// </summary>
        /// <param name="numberOfTimes">
        ///     number of times <see cref="_parameterValidatorMock.ValidateNotNullOrEmpty"/> is expected to be called.
        /// </param>
        private void VerifyCallsToParameterService(int numberOfTimes)
        {
            _parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(numberOfTimes));
        }
    }
}
