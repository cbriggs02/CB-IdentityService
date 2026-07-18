using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Shared.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace IdentityServiceApi.UnitTests.Features.Authorization.RoleServiceTests
{
    public partial class RoleServiceTests
    {
        public class AssignRoleAsync : RoleServiceTests
        {
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
    }
}
