using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Authentication;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.ServiceResultModels.UserManagement;
using IdentityServiceApi.Services.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;

namespace IdentityServiceApi.Tests.Unit.Services.Authorization
{
    /// <summary>
    ///     Unit tests for the <see cref="AuthorizationService"/> class.
    ///     This class contains test cases for various authorization scenarios, verifying the 
    ///     behavior of the authorization functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class AuthorizationServiceTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IUserContextService> _userContextServiceMock;
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
        private readonly AuthorizationService _authorizationService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthorizationServiceTests"/> class
        ///     and sets up necessary mock dependencies.
        /// </summary>
        public AuthorizationServiceTests()
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

            _userContextServiceMock = new Mock<IUserContextService>();
            _userLookupServiceMock = new Mock<IUserLookupService>();

            _authorizationService = new AuthorizationService(_userManagerMock.Object, _userContextServiceMock.Object, _userLookupServiceMock.Object);
        }

        /// <summary>
        ///     Tests that an <see cref="ArgumentNullException"/> is thrown when <see cref="AuthorizationService"/> is 
        ///     instantiated with a null dependencies.
        /// </summary>
        [Fact]
        public void AuthorizationService_NullDependencies_ThrowsArgumentNullException()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AuthorizationService(null, null, null));
        }

        /// <summary>
        ///     Tests if an admin user is able to access data of a non-admin user.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ValidatePermission_AdminAccessingNonAdmin_ReturnsTrue()
        {
            // Arrange
            const string currentUserId = "adminId";
            const string currentUserRole = Roles.Admin;

            const string targetUserId = "nonAdminId";
            const string targetUserRole = Roles.User;

            var claimsPrincipal = CreateClaimsPrinciple(currentUserId, currentUserRole);

            ArrangeClaimsPrinciple(claimsPrincipal);
            ArrangeGetUserIdFromClaims(currentUserId, claimsPrincipal);
            ArrangeGetRolesForClaimsPrincipal(claimsPrincipal, currentUserRole);

            var targetUser = new User { Id = targetUserId };

            ArrangeUserLookupResult(targetUser);
            ArrangeGetRolesForUser(targetUser, targetUserRole);

            // Act
            var result = await _authorizationService.ValidatePermissionAsync(targetUserId);

            // Assert
            Assert.True(result);

            VerifyCallsToUserContextService(claimsPrincipal);
            VerifyCallsToUserContextServiceForRoles(claimsPrincipal);
            VerifyCallsToUserLookupService(targetUser.Id);
        }

        /// <summary>
        ///     Tests if an admin user is able to access data of a admin user.
        ///     Ensuring admins cannot access other admin data.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermission_AdminAccessingAdmin_ReturnsFalse()
        {
            // Arrange
            const string currentUserId = "adminId1";
            const string currentUserRole = Roles.Admin;

            const string targetUserId = "adminId2";
            const string targetUserRole = Roles.Admin;

            var claimsPrincipal = CreateClaimsPrinciple(currentUserId, currentUserRole);

            ArrangeClaimsPrinciple(claimsPrincipal);
            ArrangeGetUserIdFromClaims(currentUserId, claimsPrincipal);
            ArrangeGetRolesForClaimsPrincipal(claimsPrincipal, currentUserRole);

            var targetUser = new User { Id = targetUserId };

            ArrangeUserLookupResult(targetUser);
            ArrangeGetRolesForUser(targetUser, targetUserRole);

            // Act
            var result = await _authorizationService.ValidatePermissionAsync(targetUserId);

            // Assert
            Assert.False(result);

            VerifyCallsToUserContextService(claimsPrincipal);
            VerifyCallsToUserContextServiceForRoles(claimsPrincipal);
            VerifyCallsToUserLookupService(targetUser.Id);
        }

        /// <summary>
        ///     Tests if an super admin user is able to access data of a admin user.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermission_SuperAdminAccessingAdmin_ReturnsTrue()
        {
            // Arrange
            const string currentUserId = "superId";
            const string currentUserRole = Roles.SuperAdmin;

            const string targetUserId = "adminId";
            const string targetUserRole = Roles.Admin;

            var claimsPrincipal = CreateClaimsPrinciple(currentUserId, currentUserRole);

            ArrangeClaimsPrinciple(claimsPrincipal);
            ArrangeGetUserIdFromClaims(currentUserId, claimsPrincipal);
            ArrangeGetRolesForClaimsPrincipal(claimsPrincipal, currentUserRole);

            var targetUser = new User { Id = targetUserId };

            ArrangeUserLookupResult(targetUser);
            ArrangeGetRolesForUser(targetUser, targetUserRole);

            // Act
            var result = await _authorizationService.ValidatePermissionAsync(targetUserId);

            // Assert
            Assert.True(result);

            VerifyCallsToUserContextService(claimsPrincipal);
            VerifyCallsToUserContextServiceForRoles(claimsPrincipal);
        }

        /// <summary>
        ///     Tests if an super admin user is able to access data of a user.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermission_SuperAdminAccessingUser_ReturnsTrue()
        {
            // Arrange
            const string currentUserId = "superId";
            const string currentUserRole = Roles.SuperAdmin;

            const string targetUserId = "userId";
            const string targetUserRole = Roles.User;

            var claimsPrincipal = CreateClaimsPrinciple(currentUserId, currentUserRole);

            ArrangeClaimsPrinciple(claimsPrincipal);
            ArrangeGetUserIdFromClaims(currentUserId, claimsPrincipal);
            ArrangeGetRolesForClaimsPrincipal(claimsPrincipal, currentUserRole);

            var targetUser = new User { Id = targetUserId };

            ArrangeUserLookupResult(targetUser);
            ArrangeGetRolesForUser(targetUser, targetUserRole);

            // Act
            var result = await _authorizationService.ValidatePermissionAsync(targetUserId);

            // Assert
            Assert.True(result);

            VerifyCallsToUserContextService(claimsPrincipal);
            VerifyCallsToUserContextServiceForRoles(claimsPrincipal);
        }

        /// <summary>
        ///     Tests if an super admin user is able to access data of a super admin.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermission_SuperAdminAccessingSuperAdmin_ReturnsTrue()
        {
            // Arrange
            const string currentUserId = "superId1";
            const string currentUserRole = Roles.SuperAdmin;

            const string targetUserId = "superId2";
            const string targetUserRole = Roles.SuperAdmin;

            var claimsPrincipal = CreateClaimsPrinciple(currentUserId, currentUserRole);

            ArrangeClaimsPrinciple(claimsPrincipal);
            ArrangeGetUserIdFromClaims(currentUserId, claimsPrincipal);
            ArrangeGetRolesForClaimsPrincipal(claimsPrincipal, currentUserRole);

            var targetUser = new User { Id = targetUserId };

            ArrangeUserLookupResult(targetUser);
            ArrangeGetRolesForUser(targetUser, targetUserRole);

            // Act
            var result = await _authorizationService.ValidatePermissionAsync(targetUserId);

            // Assert
            Assert.True(result);

            VerifyCallsToUserContextService(claimsPrincipal);
            VerifyCallsToUserContextServiceForRoles(claimsPrincipal);
        }

        /// <summary>
        ///     Tests if an super admin user is able to access their own data.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermission_SuperAdminAccessingSelf_ReturnsTrue()
        {
            // Arrange
            const string currentUserId = "id";
            const string currentUserRole = Roles.SuperAdmin;

            const string targetUserId = "id";
            const string targetUserRole = Roles.SuperAdmin;

            var claimsPrincipal = CreateClaimsPrinciple(currentUserId, currentUserRole);

            ArrangeClaimsPrinciple(claimsPrincipal);
            ArrangeGetUserIdFromClaims(currentUserId, claimsPrincipal);
            ArrangeGetRolesForClaimsPrincipal(claimsPrincipal, currentUserRole);

            var targetUser = new User { Id = targetUserId };

            ArrangeUserLookupResult(targetUser);
            ArrangeGetRolesForUser(targetUser, targetUserRole);

            // Act
            var result = await _authorizationService.ValidatePermissionAsync(targetUserId);

            // Assert
            Assert.True(result);

            VerifyCallsToUserContextService(claimsPrincipal);
            VerifyCallsToUserContextServiceForRoles(claimsPrincipal);
        }

        /// <summary>
        ///     Tests if an admin user is able to access their own data.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermission_AdminAccessingSelf_ReturnsTrue()
        {
            // Arrange
            const string currentUserId = "id";
            const string currentUserRole = Roles.Admin;

            const string targetUserId = "id";
            const string targetUserRole = Roles.Admin;

            var claimsPrincipal = CreateClaimsPrinciple(currentUserId, currentUserRole);

            ArrangeClaimsPrinciple(claimsPrincipal);
            ArrangeGetUserIdFromClaims(currentUserId, claimsPrincipal);
            ArrangeGetRolesForClaimsPrincipal(claimsPrincipal, currentUserRole);

            var targetUser = new User { Id = targetUserId };

            ArrangeUserLookupResult(targetUser);
            ArrangeGetRolesForUser(targetUser, targetUserRole);

            // Act
            var result = await _authorizationService.ValidatePermissionAsync(targetUserId);

            // Assert
            Assert.True(result);

            VerifyCallsToUserContextService(claimsPrincipal);
            VerifyCallsToUserContextServiceForRoles(claimsPrincipal);
            VerifyCallsToUserLookupService(targetUser.Id);
        }

        /// <summary>
        ///     Tests if a user is able to access their own data.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermission_UserAccessingSelf_ReturnsTrue()
        {
            // Arrange
            const string currentUserId = "id";
            const string currentUserRole = Roles.User;

            const string targetUserId = "id";
            const string targetUserRole = Roles.User;

            var claimsPrincipal = CreateClaimsPrinciple(currentUserId, currentUserRole);

            ArrangeClaimsPrinciple(claimsPrincipal);
            ArrangeGetUserIdFromClaims(currentUserId, claimsPrincipal);
            ArrangeGetRolesForClaimsPrincipal(claimsPrincipal, currentUserRole);

            var targetUser = new User { Id = targetUserId };

            ArrangeUserLookupResult(targetUser);
            ArrangeGetRolesForUser(targetUser, targetUserRole);

            // Act
            var result = await _authorizationService.ValidatePermissionAsync(targetUserId);

            // Assert
            Assert.True(result);

            VerifyCallsToUserContextService(claimsPrincipal);
            VerifyCallsToUserContextServiceForRoles(claimsPrincipal);
        }

        /// <summary>
        ///     Tests if a user is able to access their own data when they have no roles assigned.
        ///     Ensures users who have no assigned role can access any data.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermission_UserAccessingSelfWithNoRoles_ReturnsFalse()
        {
            // Arrange
            const string currentUserId = "id";
            const string targetUserId = "id";

            var claimsPrincipal = CreateClaimsPrinciple(currentUserId, "");

            ArrangeClaimsPrinciple(claimsPrincipal);
            ArrangeGetUserIdFromClaims(currentUserId, claimsPrincipal);

            var targetUser = new User { Id = targetUserId };

            ArrangeUserLookupResult(targetUser);

            // Act
            var result = await _authorizationService.ValidatePermissionAsync(targetUserId);

            // Assert
            Assert.False(result);

            VerifyCallsToUserContextService(claimsPrincipal);
        }

        /// <summary>
        ///     Tests if a user is able to access other users data when they have no roles assigned.
        ///     Ensures users who have no assigned role can access any data.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermission_UserAccessingOtherWithNoRoles_ReturnsFalse()
        {
            // Arrange
            const string currentUserId = "id";
            const string targetUserId = "id2";

            var claimsPrincipal = CreateClaimsPrinciple(currentUserId, "");

            ArrangeClaimsPrinciple(claimsPrincipal);
            ArrangeGetUserIdFromClaims(currentUserId, claimsPrincipal);

            var targetUser = new User { Id = targetUserId };

            ArrangeUserLookupResult(targetUser);

            // Act
            var result = await _authorizationService.ValidatePermissionAsync(targetUserId);

            // Assert
            Assert.False(result);

            VerifyCallsToUserContextService(claimsPrincipal);
        }

        /// <summary>
        ///     Tests if a user is able to access another users data.
        ///     Ensures users cannot access other users data.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermission_UserAccessingUser_ReturnsFalse()
        {
            // Arrange
            const string currentUserId = "userId1";
            const string currentUserRole = Roles.User;

            const string targetUserId = "userId2";
            const string targetUserRole = Roles.User;

            var claimsPrincipal = CreateClaimsPrinciple(currentUserId, currentUserRole);

            ArrangeClaimsPrinciple(claimsPrincipal);
            ArrangeGetUserIdFromClaims(currentUserId, claimsPrincipal);
            ArrangeGetRolesForClaimsPrincipal(claimsPrincipal, currentUserRole);

            var targetUser = new User { Id = targetUserId };

            ArrangeUserLookupResult(targetUser);
            ArrangeGetRolesForUser(targetUser, targetUserRole);

            // Act
            var result = await _authorizationService.ValidatePermissionAsync(targetUserId);

            // Assert
            Assert.False(result);

            VerifyCallsToUserContextService(claimsPrincipal);
            VerifyCallsToUserContextServiceForRoles(claimsPrincipal);
        }

        /// <summary>
        ///     Tests if a user is able to access a admins data.
        ///     Ensures users cannot access admin data.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermission_UserAccessingAdmin_ReturnsFalse()
        {
            // Arrange
            const string currentUserId = "userId";
            const string currentUserRole = Roles.User;

            const string targetUserId = "adminId";
            const string targetUserRole = Roles.Admin;

            var claimsPrincipal = CreateClaimsPrinciple(currentUserId, currentUserRole);

            ArrangeClaimsPrinciple(claimsPrincipal);
            ArrangeGetUserIdFromClaims(currentUserId, claimsPrincipal);
            ArrangeGetRolesForClaimsPrincipal(claimsPrincipal, currentUserRole);

            var targetUser = new User { Id = targetUserId };

            ArrangeUserLookupResult(targetUser);
            ArrangeGetRolesForUser(targetUser, targetUserRole);

            // Act
            var result = await _authorizationService.ValidatePermissionAsync(targetUserId);

            // Assert
            Assert.False(result);

            VerifyCallsToUserContextService(claimsPrincipal);
            VerifyCallsToUserContextServiceForRoles(claimsPrincipal);
        }

        /// <summary>
        ///     Tests if a user is able to access a super admins data.
        ///     Ensures users cannot access super admin data.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermission_UserAccessingSuperAdmin_ReturnsFalse()
        {
            // Arrange
            const string currentUserId = "userId";
            const string currentUserRole = Roles.User;

            const string targetUserId = "superAdminId";
            const string targetUserRole = Roles.SuperAdmin;

            var claimsPrincipal = CreateClaimsPrinciple(currentUserId, currentUserRole);

            ArrangeClaimsPrinciple(claimsPrincipal);
            ArrangeGetUserIdFromClaims(currentUserId, claimsPrincipal);
            ArrangeGetRolesForClaimsPrincipal(claimsPrincipal, currentUserRole);

            var targetUser = new User { Id = targetUserId };

            ArrangeUserLookupResult(targetUser);
            ArrangeGetRolesForUser(targetUser, targetUserRole);

            // Act
            var result = await _authorizationService.ValidatePermissionAsync(targetUserId);

            // Assert
            Assert.False(result);

            VerifyCallsToUserContextService(claimsPrincipal);
            VerifyCallsToUserContextServiceForRoles(claimsPrincipal);
        }

        /// <summary>
        ///     Tests if accessing a non-existent user returns false.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermission_AccessingNonExistentUser_ReturnsFalse()
        {
            // Arrange
            const string currentUserId = "existingUserId";
            const string currentUserRole = Roles.User;

            const string targetUserId = "nonExistentUserId";

            var claimsPrincipal = CreateClaimsPrinciple(currentUserId, currentUserRole);

            ArrangeClaimsPrinciple(claimsPrincipal);
            ArrangeGetUserIdFromClaims(currentUserId, claimsPrincipal);
            ArrangeGetRolesForClaimsPrincipal(claimsPrincipal, currentUserRole);

            _userLookupServiceMock
                .Setup(x => x.FindUserByIdAsync(targetUserId))
                .ReturnsAsync(new UserLookupServiceResult
                {
                    Success = false,
                    Errors = new[] { ErrorMessages.User.NotFound }.ToList()
                });

            // Act
            var result = await _authorizationService.ValidatePermissionAsync(targetUserId);

            // Assert
            Assert.False(result);

            VerifyCallsToUserContextService(claimsPrincipal);
            VerifyCallsToUserContextServiceForRoles(claimsPrincipal);
        }

        /// <summary>
        ///     Tests that the <see cref="AuthorizationService.ValidatePermissionAsync(string)"/> method
        ///     returns false when the claims principal is null.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermission_NullClaimsPrinciple_ReturnsFalse()
        {
            // Arrange
            const string targetUserId = "nonAdminId";
            const string targetUserRole = Roles.User;

            ClaimsPrincipal claimsPrincipal = null;

            ArrangeClaimsPrinciple(claimsPrincipal);

            var targetUser = new User { Id = targetUserId };

            ArrangeUserLookupResult(targetUser);
            ArrangeGetRolesForUser(targetUser, targetUserRole);

            // Act
            var result = await _authorizationService.ValidatePermissionAsync(targetUserId);

            // Assert
            Assert.False(result);

            _userContextServiceMock.Verify(p => p.GetClaimsPrincipal(), Times.Once);
        }

        /// <summary>
        ///     Tests that the <see cref="AuthorizationService.ValidatePermissionAsync(string)"/> method
        ///     returns false when the target user ID is null.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermission_NullId_ReturnsFalse()
        {
            // Arrange
            const string currentUserId = "";
            const string currentUserRole = Roles.Admin;

            var claimsPrincipal = CreateClaimsPrinciple(currentUserId, currentUserRole);

            ArrangeClaimsPrinciple(claimsPrincipal);

            // Act
            var result = await _authorizationService.ValidatePermissionAsync(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        ///     Tests that the <see cref="AuthorizationService.ValidatePermissionAsync(string)"/> method
        ///     returns false when both the claims principal and target user ID are null.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task ValidatePermission_NullClaimsPrincipleAndNullId_ReturnsFalse()
        {
            // Arrange
            ClaimsPrincipal claimsPrincipal = null;

            ArrangeClaimsPrinciple(claimsPrincipal);

            // Act
            var result = await _authorizationService.ValidatePermissionAsync(null);

            // Assert
            Assert.False(result);
        }

        private static ClaimsPrincipal CreateClaimsPrinciple(string currentUserId, string role)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.NameIdentifier, currentUserId),
                new(ClaimTypes.Role, role)
            }));
        }

        private void ArrangeClaimsPrinciple(ClaimsPrincipal claimsPrincipal)
        {
            _userContextServiceMock
                .Setup(x => x.GetClaimsPrincipal())
                .Returns(claimsPrincipal);
        }

        private void ArrangeGetUserIdFromClaims(string currentUserId, ClaimsPrincipal claimsPrincipal)
        {
            _userContextServiceMock
                .Setup(x => x.GetUserId(claimsPrincipal))
                .Returns(currentUserId);
        }

        private void ArrangeGetRolesForClaimsPrincipal(ClaimsPrincipal claimsPrincipal, string role)
        {
            _userContextServiceMock
                .Setup(x => x.GetRoles(claimsPrincipal))
                .Returns(new List<string> { role });
        }

        private void ArrangeGetRolesForUser(User targetUser, string role)
        {
            _userManagerMock
                .Setup(x => x.GetRolesAsync(targetUser))
                .ReturnsAsync(new List<string> { role });
        }

        private void ArrangeUserLookupResult(User user)
        {
            var result = new UserLookupServiceResult
            {
                Success = true,
                UserFound = user,
            };

            _userLookupServiceMock
                .Setup(x => x.FindUserByIdAsync(user.Id))
                .ReturnsAsync(result);
        }

        private void VerifyCallsToUserContextService(ClaimsPrincipal claimsPrincipal)
        {
            _userContextServiceMock.Verify(p => p.GetClaimsPrincipal(), Times.Once);
            _userContextServiceMock.Verify(u => u.GetUserId(claimsPrincipal), Times.Once);
        }

        private void VerifyCallsToUserContextServiceForRoles(ClaimsPrincipal claimsPrincipal)
        {
            _userContextServiceMock.Verify(r => r.GetRoles(claimsPrincipal), Times.Once);
        }

        private void VerifyCallsToUserLookupService(string id)
        {
            _userLookupServiceMock.Verify(x => x.FindUserByIdAsync(id), Times.Once);
        }
    }
}
