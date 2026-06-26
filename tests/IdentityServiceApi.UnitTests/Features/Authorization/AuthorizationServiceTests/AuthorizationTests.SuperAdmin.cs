using IdentityServiceApi.Shared.Constants;

namespace IdentityServiceApi.UnitTests.Features.Authorization.AuthorizationTests
{
    public partial class AuthorizationServiceTests
    {
        public class SuperAdminTests : AuthorizationServiceTests
        {
            public async Task ValidatePermissionAsync_SuperAdmin_HasAccess()
            {
                var principal = CreatePrincipal("1", Roles.SuperAdmin);
                SetupContext(principal);

                var result = await _service.ValidatePermissionAsync("any-id");
                Assert.True(result);
            }
        }
    }
}
