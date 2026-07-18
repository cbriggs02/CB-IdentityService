using IdentityServiceApi.Features.Authorization.Models;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Results;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace IdentityServiceApi.UnitTests.Features.Authorization.RoleServiceTests
{
    public partial class RoleServiceTests
    {
        public class GetRoleAsync : RoleServiceTests
        {
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
    }
}
