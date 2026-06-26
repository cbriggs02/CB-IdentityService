using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.Results;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace IdentityServiceApi.UnitTests.Features.UserManagement.UserServiceTests
{
    public partial class UserServiceTests
    {
        public class DeleteUserAsyncTests : UserServiceTests
        {
            [Fact]
            public async Task DeleteUserAsync_Success_DeletesUserAndHistory()
            {
                var user = new User { Id = "id" };

                _permissions
                   .Setup(x => x.ValidatePermissionsAsync("id"))
                   .ReturnsAsync(new Result { Success = true });

                _lookup
                    .Setup(x => x.FindUserByIdAsync("id"))
                    .ReturnsAsync(new UserLookupResult
                    {
                        Success = true,
                        UserFound = user
                    });

                _userManager.Setup(x => x.DeleteAsync(user))
                    .ReturnsAsync(IdentityResult.Success);

                _resultFactory.Setup(x => x.GeneralOperationSuccess())
                    .Returns(new Result { Success = true });

                var result = await _service.DeleteUserAsync("id");
                Assert.True(result.Success);

                _cleanup.Verify(x => x.DeletePasswordHistoryAsync("id"), Times.Once);
                _cacheService.Verify(x => x.ClearUserListCache(), Times.Once);
            }
        }
    }
}
