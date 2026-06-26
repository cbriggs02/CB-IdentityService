using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Features.Authorization.Models;
using IdentityServiceApi.Features.Authorization.Services;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Shared.Results;
using IdentityServiceApi.Shared.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace IdentityServiceApi.UnitTests.Features.Authorization
{
    /// <summary>
    ///    Unit tests for the <see cref="RoleService"/> class.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2026
    ///     @Updated: 2026
    /// </remarks>
    public class RoleServiceTests
    {
        protected readonly Mock<IMemoryCache> _cache = new();
        protected readonly Mock<RoleManager<IdentityRole>> _roleManager = RoleManagerMock();
        protected readonly Mock<UserManager<User>> _userManager = UserManagerMock();
        protected readonly Mock<IParameterValidator> _validator = new();
        protected readonly Mock<IRoleResultFactory> _factory = new();
        protected readonly Mock<IUserLookupService> _userLookup = new();
        protected readonly Mock<ILoggerService> _logger = new();
        protected readonly RoleService _service;

        /// <summary>
        ///    Initializes a new instance of the <see cref="RoleServiceTests"/> class and sets up the necessary mocks for testing.
        /// </summary>
        public RoleServiceTests()
        {
            _service = new RoleService(
                _cache.Object,
                _roleManager.Object,
                _userManager.Object,
                _validator.Object,
                _factory.Object,
                _userLookup.Object,
                _logger.Object);
        }

        /// <summary>
        ///     Tests scenarios related to retrieving a single role by ID.
        ///     Includes success and failure cases.
        /// </summary>
        public class GetRole : RoleServiceTests
        {
            /// <summary>
            ///     Verifies that <see cref="RoleService.GetRoleAsync(string)"/> returns a failure result
            ///     when the specified role does not exist.
            /// </summary>
            [Fact]
            public async Task GetRoleAsync_RoleNotFound_ReturnsFailure()
            {
                _roleManager
                    .Setup(x => x.FindByIdAsync("1"))
                    .ReturnsAsync((IdentityRole)null);

                _factory
                    .Setup(x => x.RoleOperationFailure(It.IsAny<string[]>(), ErrorType.NotFound))
                    .Returns(new RoleResult { Success = false });

                var result = await _service.GetRoleAsync("1");
                Assert.False(result.Success);
            }

            /// <summary>
            ///     Verifies that <see cref="RoleService.GetRoleAsync(string)"/> returns a success result
            ///     when the role exists.
            /// </summary>
            [Fact]
            public async Task GetRoleAsync_RoleExists_ReturnsSuccess()
            {
                var role = new IdentityRole { Id = "1", Name = Roles.Admin };

                _roleManager
                    .Setup(x => x.FindByIdAsync("1"))
                    .ReturnsAsync(role);

                _factory
                    .Setup(x => x.RoleOperationSuccess(It.IsAny<RoleDTO>()))
                    .Returns(new RoleResult { Success = true });

                var result = await _service.GetRoleAsync("1");
                Assert.True(result.Success);
            }
        }

        /// <summary>
        ///     Tests scenarios related to assigning roles to users,
        ///     including validation, edge cases, and successful assignments.
        /// </summary>
        public class AssignRole : RoleServiceTests
        {
            /// <summary>
            ///     Verifies that role assignment fails when the specified user cannot be found.
            /// </summary>
            [Fact]
            public async Task AssignRoleAsync_UserNotFound_ReturnsFailure()
            {
                _userLookup
                    .Setup(x => x.FindUserByIdAsync("1"))
                    .ReturnsAsync(new UserLookupResult { Success = false, Errors = [ErrorMessages.Role.NotFound] });

                _factory
                    .Setup(x => x.GeneralOperationFailure(It.IsAny<string[]>(), It.IsAny<ErrorType>()))
                    .Returns(new Result { Success = false });

                var result = await _service.AssignRoleAsync("1", Roles.Admin);
                Assert.False(result.Success);
            }

            /// <summary>
            ///     Verifies that role assignment fails when the target user is inactive.
            /// </summary>
            [Fact]
            public async Task AssignRoleAsync_InactiveUser_ReturnsFailure()
            {
                var user = new User { Id = "1", AccountStatus = 0 };

                _userLookup
                    .Setup(x => x.FindUserByIdAsync("1"))
                    .ReturnsAsync(new UserLookupResult { Success = true, UserFound = user });

                _factory
                    .Setup(x => x.GeneralOperationFailure(It.IsAny<string[]>(), ErrorType.InvalidState))
                    .Returns(new Result { Success = false });

                var result = await _service.AssignRoleAsync("1", Roles.Admin);
                Assert.False(result.Success);
            }

            /// <summary>
            ///     Verifies that role assignment fails when the provided role does not exist.
            /// </summary>
            [Fact]
            public async Task AssignRoleAsync_InvalidRole_ReturnsFailure()
            {
                const string role = Roles.Admin;
                var user = new User { Id = "1", AccountStatus = 1 };

                _userLookup
                    .Setup(x => x.FindUserByIdAsync("1"))
                    .ReturnsAsync(new UserLookupResult { Success = true, UserFound = user });

                _roleManager
                    .Setup(x => x.RoleExistsAsync(role))
                    .ReturnsAsync(false);

                _factory
                    .Setup(x => x.GeneralOperationFailure(It.IsAny<string[]>(), ErrorType.Validation))
                    .Returns(new Result { Success = false });

                var result = await _service.AssignRoleAsync("1", role);
                Assert.False(result.Success);
            }

            /// <summary>
            ///     Verifies that role assignment fails when the user already has a role assigned.
            /// </summary>
            [Fact]
            public async Task AssignRoleAsync_UserAlreadyHasRole_ReturnsFailure()
            {
                const string role = Roles.Admin;
                var user = new User { Id = "1", AccountStatus = 1 };

                _userLookup
                    .Setup(x => x.FindUserByIdAsync("1"))
                    .ReturnsAsync(new UserLookupResult { Success = true, UserFound = user });

                _roleManager.Setup(x => x.RoleExistsAsync(role)).ReturnsAsync(true);

                _userManager
                    .Setup(x => x.GetRolesAsync(user))
                    .ReturnsAsync([role]);

                _factory
                    .Setup(x => x.GeneralOperationFailure(It.IsAny<string[]>(), ErrorType.InvalidState))
                    .Returns(new Result { Success = false });

                var result = await _service.AssignRoleAsync("1", role);
                Assert.False(result.Success);
            }

            /// <summary>
            ///     Verifies that role assignment fails when adding the role through the identity framework fails.
            /// </summary>
            [Fact]
            public async Task AssignRoleAsync_AddRoleFails_ReturnsFailure()
            {
                const string role = Roles.Admin;
                var user = new User { Id = "1", AccountStatus = 1 };

                _userLookup.Setup(x => x.FindUserByIdAsync("1"))
                    .ReturnsAsync(new UserLookupResult { Success = true, UserFound = user });

                _roleManager.Setup(x => x.RoleExistsAsync(role)).ReturnsAsync(true);

                _userManager.Setup(x => x.GetRolesAsync(user))
                    .ReturnsAsync([]);

                _userManager.Setup(x => x.AddToRoleAsync(user, role))
                    .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "fail" }));

                _factory
                    .Setup(x => x.GeneralOperationFailure(It.IsAny<string[]>(), ErrorType.Validation))
                    .Returns(new Result { Success = false });

                var result = await _service.AssignRoleAsync("1", role);
                Assert.False(result.Success);
            }

            /// <summary>
            ///     Verifies that a role is successfully assigned to a user and the operation is logged.
            /// </summary>
            [Fact]
            public async Task AssignRoleAsync_Success_LogsAndReturnsSuccess()
            {
                const string role = Roles.Admin;
                var user = new User { Id = "1", AccountStatus = 1 };

                _userLookup.Setup(x => x.FindUserByIdAsync("1"))
                    .ReturnsAsync(new UserLookupResult { Success = true, UserFound = user });

                _roleManager.Setup(x => x.RoleExistsAsync(role)).ReturnsAsync(true);

                _userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync([]);

                _userManager.Setup(x => x.AddToRoleAsync(user, role))
                    .ReturnsAsync(IdentityResult.Success);

                _factory
                    .Setup(x => x.GeneralOperationSuccess())
                    .Returns(new Result { Success = true });

                var result = await _service.AssignRoleAsync("1", role);
                Assert.True(result.Success);

                _logger.Verify(x =>
                    x.LogData(It.Is<LogEntry>(l =>
                        l.Message.Contains("Assigned role") &&
                        l.LogLevel == LogLevel.Information)),
                    Times.Once);
            }
        }

        /// <summary>
        ///     Tests scenarios related to removing roles from users,
        ///     including validation, error cases, and successful removal.
        /// </summary>
        public class RemoveRole : RoleServiceTests
        {
            /// <summary>
            ///     Verifies that role removal fails when the specified user cannot be found.
            /// </summary>
            [Fact]
            public async Task RemoveRoleAsync_UserNotFound_ReturnsFailure()
            {
                _userLookup
                    .Setup(x => x.FindUserByIdAsync("1"))
                    .ReturnsAsync(new UserLookupResult { Success = false, Errors = [ErrorMessages.User.NotFound] });

                _factory
                    .Setup(x => x.GeneralOperationFailure(It.IsAny<string[]>(), It.IsAny<ErrorType>()))
                    .Returns(new Result { Success = false });

                var result = await _service.RemoveRoleAsync("1");
                Assert.False(result.Success);
            }

            /// <summary>
            ///     Verifies that role removal fails when the user does not have any assigned roles.
            /// </summary>
            [Fact]
            public async Task RemoveRoleAsync_NoRoleAssigned_ReturnsFailure()
            {
                var user = new User { Id = "1" };

                _userLookup
                    .Setup(x => x.FindUserByIdAsync("1"))
                    .ReturnsAsync(new UserLookupResult { Success = true, UserFound = user });

                _userManager
                    .Setup(x => x.GetRolesAsync(user))
                    .ReturnsAsync([]);

                _factory
                    .Setup(x => x.GeneralOperationFailure(It.IsAny<string[]>(), ErrorType.InvalidState))
                    .Returns(new Result { Success = false });

                var result = await _service.RemoveRoleAsync("1");
                Assert.False(result.Success);
            }

            /// <summary>
            ///     Verifies that role removal fails when the underlying identity operation fails.
            /// </summary>
            [Fact]
            public async Task RemoveRoleAsync_RemoveFails_ReturnsFailure()
            {
                const string role = Roles.Admin;
                var user = new User { Id = "1" };

                _userLookup.Setup(x => x.FindUserByIdAsync("1"))
                    .ReturnsAsync(new UserLookupResult { Success = true, UserFound = user });

                _userManager.Setup(x => x.GetRolesAsync(user))
                    .ReturnsAsync([role]);

                _userManager.Setup(x => x.RemoveFromRoleAsync(user, role))
                    .ReturnsAsync(IdentityResult.Failed());

                _factory
                    .Setup(x => x.GeneralOperationFailure(It.IsAny<string[]>(), ErrorType.Validation))
                    .Returns(new Result { Success = false });

                var result = await _service.RemoveRoleAsync("1");
                Assert.False(result.Success);
            }

            /// <summary>
            ///     Verifies that a role is successfully removed from a user and the operation is logged.
            /// </summary>
            [Fact]
            public async Task RemoveRoleAsync_Success_LogsAndReturnsSuccess()
            {
                const string role = Roles.Admin;
                var user = new User { Id = "1" };

                _userLookup.Setup(x => x.FindUserByIdAsync("1"))
                    .ReturnsAsync(new UserLookupResult { Success = true, UserFound = user });

                _userManager.Setup(x => x.GetRolesAsync(user))
                    .ReturnsAsync([role]);

                _userManager.Setup(x => x.RemoveFromRoleAsync(user, role))
                    .ReturnsAsync(IdentityResult.Success);

                _factory
                    .Setup(x => x.GeneralOperationSuccess())
                    .Returns(new Result { Success = true });

                var result = await _service.RemoveRoleAsync("1");
                Assert.True(result.Success);

                _logger.Verify(x =>
                    x.LogData(It.Is<LogEntry>(l =>
                        l.Message.Contains("Removed role") &&
                        l.LogLevel == LogLevel.Information)),
                    Times.Once);
            }
        }

        private static Mock<UserManager<User>> UserManagerMock()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                store.Object,
                null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private static Mock<RoleManager<IdentityRole>> RoleManagerMock()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(
                store.Object,
                null!, null!, null!, null!);
        }
    }
}