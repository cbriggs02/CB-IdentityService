using IdentityServiceApi.Features.UserManagement.Models.DTOs;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Results;
using Moq;

namespace IdentityServiceApi.UnitTests.Features.UserManagement.UserServiceTests
{
    public partial class UserServiceTests
    {
        public class GetUserAsyncTests : UserServiceTests
        {
            [Fact]
            public async Task GetUserAsync_PermissionFail_ReturnsFailure()
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

                var result = await _service.GetUserAsync("id");
                Assert.False(result.Success);
            }

            [Fact]
            public async Task GetUserAsync_UserNotFound_ReturnsFailure()
            {
                const string expectedError = ErrorMessages.User.NotFound;
                var expectedErrorType = ErrorType.NotFound;

                _permissions
                   .Setup(x => x.ValidatePermissionsAsync("id"))
                   .ReturnsAsync(new Result { Success = true });

                _lookup.Setup(x => x.FindUserByIdAsync("id"))
                    .ReturnsAsync(new UserLookupResult
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

                var result = await _service.GetUserAsync("id");
                Assert.False(result.Success);
            }

            [Fact]
            public async Task GetUserAsync_Success_ReturnsUser()
            {
                var user = new User { Id = "id" };
                var dto = new UserDTO();

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

                _mapper.Setup(x => x.Map<UserDTO>(user))
                    .Returns(dto);

                _userManager.Setup(x => x.GetRolesAsync(user))
                    .ReturnsAsync([]);

                _resultFactory.Setup(x => x.UserOperationSuccess(dto))
                    .Returns(new UserResult { Success = true, User = dto });

                var result = await _service.GetUserAsync("id");
                Assert.True(result.Success);
            }
        }
    }
}
