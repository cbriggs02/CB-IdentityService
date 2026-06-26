using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Results;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace IdentityServiceApi.UnitTests.Features.UserManagement.UserServiceTests
{
    public partial class UserServiceTests
    {
        public class UpdateUserAsyncTests : UserServiceTests
        {
            [Fact]
            public async Task UpdateUserAsync_PermissionFail_ReturnsFailure()
            {
                const string expectedError = ErrorMessages.Authorization.Unauthorized;
                var expectedErrorType = ErrorType.Unauthorized;

                _permissions
                    .Setup(x => x.ValidatePermissionsAsync("id"))
                    .ReturnsAsync(new Result
                    {
                        Success = false,
                        Errors = [expectedError],
                        ErrorType = expectedErrorType
                    });

                var expected = new UserResult
                {
                    Success = false,
                    Errors = [expectedError],
                    ErrorType = expectedErrorType
                };

                _resultFactory.Setup(x =>
                    x.UserOperationFailure(It.IsAny<string[]>(), expectedErrorType))
                    .Returns(expected);

                var result = await _service.UpdateUserAsync("id", CreateValidDto());
                Assert.False(result.Success);
            }

            [Fact]
            public async Task UpdateUserAsync_Success_UpdatesUser()
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

                _country.Setup(x => x.FindCountryByIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(new Country());

                _userManager.Setup(x => x.UpdateAsync(user))
                    .ReturnsAsync(IdentityResult.Success);

                _resultFactory.Setup(x => x.GeneralOperationSuccess())
                    .Returns(new Result { Success = true });

                var result = await _service.UpdateUserAsync("id", CreateValidDto());
                Assert.True(result.Success);

                _cacheService.Verify(x => x.ClearUserListCache(), Times.Once);
            }
        }
    }
}
