using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.Constants;
using Moq;

namespace IdentityServiceApi.UnitTests.Features.Authorization.AuthorizationTests
{
    public partial class AuthorizationServiceTests
    {
        public class AdminTests : AuthorizationServiceTests
        {
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
    }
}
