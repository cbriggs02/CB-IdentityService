
namespace IdentityServiceApi.UnitTests.Features.Authorization.AuthorizationTests
{
    public partial class AuthorizationServiceTests
    {
        public class UserTests : AuthorizationServiceTests
        {
            [Fact]
            public async Task ValidatePermissionAsync_User_SelfAccess_ReturnsTrue()
            {
                var principal = CreatePrincipal("1", "User");
                SetupContext(principal);

                var result = await _service.ValidatePermissionAsync("1");
                Assert.True(result);
            }

            [Fact]
            public async Task ValidatePermissionAsync_User_NotSelf_ReturnsFalse()
            {
                var principal = CreatePrincipal("1", "User");
                SetupContext(principal);

                var result = await _service.ValidatePermissionAsync("2");
                Assert.False(result);
            }
        }
    }
}
