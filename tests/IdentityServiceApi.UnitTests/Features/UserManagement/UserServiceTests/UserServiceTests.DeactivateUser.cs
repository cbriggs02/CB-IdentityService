using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Results;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace IdentityServiceApi.UnitTests.Features.UserManagement.UserServiceTests
{
    public partial class UserServiceTests
    {
        public class DeactivateUserAsyncTests : UserServiceTests
        {
            [Fact]
            public async Task DeactivateUserAsync_NotActive_ReturnsFailure()
            {
                var expectedErrorType = ErrorType.InvalidState;
                var user = new User { Id = "id", AccountStatus = 0 };
                SetupPermissionAndLookup(user);

                _resultFactory.Setup(x =>
                    x.GeneralOperationFailure(It.IsAny<string[]>(), expectedErrorType))
                    .Returns(new Result
                    {
                        Success = false,
                        Errors = [ErrorMessages.User.NotActivated],
                        ErrorType = expectedErrorType
                    });

                var result = await _service.DeactivateUserAsync("id");
                Assert.False(result.Success);
            }

            [Fact]
            public async Task DeactivateUserAsync_Success()
            {
                var user = new User { Id = "id", AccountStatus = 1 };

                SetupPermissionAndLookup(user);

                _userManager.Setup(x => x.UpdateAsync(user))
                    .ReturnsAsync(IdentityResult.Success);

                _resultFactory.Setup(x => x.GeneralOperationSuccess())
                    .Returns(new Result { Success = true });

                var result = await _service.DeactivateUserAsync("id");
                Assert.True(result.Success);
            }
        }
    }
}
