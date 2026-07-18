using IdentityServiceApi.Features.UserManagement.Models.DTOs;
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
        public class CreateUserAsyncTests : UserServiceTests
        {
            [Fact]
            public async Task CreateUserAsync_CountryNotFound_ReturnsFailure()
            {
                var expectedErrorType = ErrorType.UnprocessableEntity;

                _country.Setup(x => x.FindCountryByIdAsync(It.IsAny<int>()))
                    .ReturnsAsync((Country)null);

                _resultFactory.Setup(x =>
                    x.UserOperationFailure(It.IsAny<string[]>(), expectedErrorType))
                    .Returns(new UserResult
                    {
                        Success = false,
                        Errors = [ErrorMessages.User.CountryNotFound],
                        ErrorType = expectedErrorType
                    });

                var result = await _service.CreateUserAsync(CreateValidDto());
                Assert.False(result.Success);
            }

            [Fact]
            public async Task CreateUserAsync_CreateFails_ReturnsFailure()
            {
                _country.Setup(x => x.FindCountryByIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(new Country { Name = "CA" });

                _userManager.Setup(x => x.CreateAsync(It.IsAny<User>()))
                    .ReturnsAsync(IdentityResult.Failed());

                _resultFactory.Setup(x =>
                    x.UserOperationFailure(It.IsAny<string[]>(), ErrorType.Validation))
                    .Returns(new UserResult { Success = false });

                var result = await _service.CreateUserAsync(CreateValidDto());
                Assert.False(result.Success);
            }

            [Fact]
            public async Task CreateUserAsync_Success_ReturnsUser()
            {
                _country.Setup(x => x.FindCountryByIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(new Country { Name = "CA" });

                _userManager.Setup(x => x.CreateAsync(It.IsAny<User>()))
                    .ReturnsAsync(IdentityResult.Success);

                _resultFactory.Setup(x => x.UserOperationSuccess(It.IsAny<UserDTO>()))
                    .Returns(new UserResult { Success = true });

                var result = await _service.CreateUserAsync(CreateValidDto());
                Assert.True(result.Success);

                _cacheService.Verify(x => x.ClearUserListCache(), Times.Once);
            }
        }
    }
}
