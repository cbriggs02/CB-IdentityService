using IdentityServiceApi.Features.Authorization.Services;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Context;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;

namespace IdentityServiceApi.UnitTests.Features.Authorization
{
    /// <summary>
    ///     Unit tests for the <see cref="AuthorizationService"/> class.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2026
    ///     @Updated: 2026
    /// </remarks>
    public class AuthorizationServiceTests
    {
        private readonly Mock<UserManager<User>> _userManager = UserManagerMock();
        private readonly Mock<IUserContextService> _context = new();
        private readonly Mock<IUserLookupService> _userLookup = new();
        private readonly AuthorizationService _service;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthorizationServiceTests"/> class.
        ///     Sets up the <see cref="AuthorizationService"/> with mocked dependencies
        ///     used for unit testing authorization behavior.
        /// </summary>
        public AuthorizationServiceTests()
        {
            _service = new AuthorizationService(
                _userManager.Object,
                _context.Object,
                _userLookup.Object);
        }

        private static ClaimsPrincipal CreatePrincipal(string userId, params string[] roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId)
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));
        }

        private void SetupContext(ClaimsPrincipal principal)
        {
            _context.Setup(x => x.GetClaimsPrincipal()).Returns(principal);
            _context.Setup(x => x.GetUserId(principal)).Returns(principal.FindFirstValue(ClaimTypes.NameIdentifier));
            _context.Setup(x => x.GetRoles(principal)).Returns(principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList());
        }

        /// <summary>
        ///     Verifies that <see cref="AuthorizationService.ValidatePermissionAsync(string)"/>
        ///     returns <c>false</c> when the provided identifier is null or empty.
        /// </summary>
        [Fact]
        public async Task ValidatePermissionAsync_InvalidId_ReturnsFalse()
        {
            var result = await _service.ValidatePermissionAsync("");
            Assert.False(result);
        }

        /// <summary>
        ///     Verifies that the authorization check fails when no claims principal
        ///     is available in the current user context.
        /// </summary>
        [Fact]
        public async Task ValidatePermissionAsync_NoPrincipal_ReturnsFalse()
        {
            _context.Setup(x => x.GetClaimsPrincipal()).Returns((ClaimsPrincipal)null);
            var result = await _service.ValidatePermissionAsync("123");
            Assert.False(result);
        }

        /// <summary>
        ///     Verifies that the authorization check fails when the current user ID
        ///     cannot be extracted from the claims principal.
        /// </summary>
        [Fact]
        public async Task ValidatePermissionAsync_NoUserId_ReturnsFalse()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity());

            _context.Setup(x => x.GetClaimsPrincipal()).Returns(principal);
            _context.Setup(x => x.GetUserId(principal)).Returns((string)null);

            var result = await _service.ValidatePermissionAsync("123");
            Assert.False(result);
        }

        /// <summary>
        ///     Verifies that the authorization check fails when the current user has
        ///     no roles assigned.
        /// </summary>
        [Fact]
        public async Task ValidatePermissionAsync_NoRoles_ReturnsFalse()
        {
            var principal = CreatePrincipal("1");

            SetupContext(principal);
            _context.Setup(x => x.GetRoles(principal)).Returns([]);

            var result = await _service.ValidatePermissionAsync("123");
            Assert.False(result);
        }

        /// <summary>
        ///     Contains tests for scenarios where the current user has the SuperAdmin role.
        /// </summary>
        public class When_SuperAdmin : AuthorizationServiceTests
        {
            /// <summary>
            ///     Verifies that a SuperAdmin user is granted access to any resource,
            ///     regardless of the target identifier.
            /// </summary>
            [Fact]
            public async Task ValidatePermissionAsync_SuperAdmin_HasAccess()
            {
                var principal = CreatePrincipal("1", Roles.SuperAdmin);
                SetupContext(principal);

                var result = await _service.ValidatePermissionAsync("any-id");
                Assert.True(result);
            }
        }

        /// <summary>
        ///     Contains tests for scenarios where the current user has the Admin role.
        ///     Includes validation rules for accessing other users based on role hierarchy.
        /// </summary>
        public class When_Admin : AuthorizationServiceTests
        {
            /// <summary>
            ///     Verifies that an Admin user is denied access when the target user cannot be found.
            /// </summary>
            [Fact]
            public async Task ValidatePermissionAsync_Admin_TargetUserNotFound_ReturnsFalse()
            {
                var principal = CreatePrincipal("1", Roles.Admin);
                SetupContext(principal);

                _userLookup
                    .Setup(x => x.FindUserByIdAsync("2"))
                    .ReturnsAsync(new UserLookupResult { Success = false });

                var result = await _service.ValidatePermissionAsync("2");
                Assert.False(result);
            }

            /// <summary>
            ///     Verifies that an Admin user is denied access when attempting to access
            ///     a SuperAdmin user.
            /// </summary>
            [Fact]
            public async Task ValidatePermissionAsync_Admin_TargetIsSuperAdmin_ReturnsFalse()
            {
                var principal = CreatePrincipal("1", Roles.Admin);
                SetupContext(principal);
                var target = new User { Id = "2" };

                _userLookup
                    .Setup(x => x.FindUserByIdAsync("2"))
                    .ReturnsAsync(new UserLookupResult { Success = true, UserFound = target });

                _userManager
                    .Setup(x => x.GetRolesAsync(target))
                    .ReturnsAsync([Roles.SuperAdmin]);

                var result = await _service.ValidatePermissionAsync("2");
                Assert.False(result);
            }

            /// <summary>
            ///     Verifies that an Admin user is denied access when attempting to access
            ///     another Admin user who is not themselves.
            /// </summary>
            [Fact]
            public async Task ValidatePermissionAsync_Admin_TargetIsAdmin_NotSelf_ReturnsFalse()

            {
                var principal = CreatePrincipal("1", Roles.Admin);
                SetupContext(principal);
                var target = new User { Id = "2" };

                _userLookup
                    .Setup(x => x.FindUserByIdAsync("2"))
                    .ReturnsAsync(new UserLookupResult { Success = true, UserFound = target });

                _userManager
                    .Setup(x => x.GetRolesAsync(target))
                    .ReturnsAsync([Roles.Admin]);

                var result = await _service.ValidatePermissionAsync("2");
                Assert.False(result);
            }

            /// <summary>
            ///     Verifies that an Admin user is allowed to access their own account,
            ///     even if they have Admin privileges.
            /// </summary>
            [Fact]
            public async Task ValidatePermissionAsync_Admin_TargetIsAdmin_Self_ReturnsTrue()
            {
                var principal = CreatePrincipal("1", Roles.Admin);
                SetupContext(principal);
                var target = new User { Id = "1" };

                _userLookup
                    .Setup(x => x.FindUserByIdAsync("1"))
                    .ReturnsAsync(new UserLookupResult { Success = true, UserFound = target });

                _userManager
                    .Setup(x => x.GetRolesAsync(target))
                    .ReturnsAsync([Roles.Admin]);

                var result = await _service.ValidatePermissionAsync("1");
                Assert.True(result);
            }

            /// <summary>
            ///     Verifies that an Admin user is allowed to access a regular (non-Admin, non-SuperAdmin) user.
            /// </summary>
            [Fact]
            public async Task ValidatePermissionAsync_Admin_TargetIsRegularUser_ReturnsTrue()
            {
                var principal = CreatePrincipal("1", Roles.Admin);
                SetupContext(principal);
                var target = new User { Id = "2" };

                _userLookup
                    .Setup(x => x.FindUserByIdAsync("2"))
                    .ReturnsAsync(new UserLookupResult { Success = true, UserFound = target });

                _userManager
                    .Setup(x => x.GetRolesAsync(target))
                    .ReturnsAsync([]);

                var result = await _service.ValidatePermissionAsync("2");
                Assert.True(result);
            }
        }

        /// <summary>
        ///     Contains tests for scenarios where the current user is a regular user
        ///     without elevated roles.
        /// </summary>
        public class When_RegularUser : AuthorizationServiceTests
        {
            /// <summary>
            ///     Verifies that a regular user is allowed to access their own resource (self-access).
            /// </summary>
            /// <returns>
            ///     A task representing the asynchronous test.
            /// </returns>
            [Fact]
            public async Task ValidatePermissionAsync_User_SelfAccess_ReturnsTrue()
            {
                var principal = CreatePrincipal("1", "User");
                SetupContext(principal);

                var result = await _service.ValidatePermissionAsync("1");
                Assert.True(result);
            }

            /// <summary>
            ///     Verifies that a regular user is denied access when attempting to access
            ///     another user's resource.
            /// </summary>
            /// <returns>
            ///     A task representing the asynchronous test.
            /// </returns>
            [Fact]
            public async Task ValidatePermissionAsync_User_NotSelf_ReturnsFalse()
            {
                var principal = CreatePrincipal("1", "User");
                SetupContext(principal);

                var result = await _service.ValidatePermissionAsync("2");
                Assert.False(result);
            }
        }

        private static Mock<UserManager<User>> UserManagerMock()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                store.Object,
                null!, null!, null!, null!, null!, null!, null!, null!);
        }
    }
}

