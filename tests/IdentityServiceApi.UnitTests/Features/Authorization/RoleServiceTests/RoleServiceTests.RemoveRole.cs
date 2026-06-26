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
        public class RemoveRoleAsync : RoleServiceTests
        {
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
    }
}
