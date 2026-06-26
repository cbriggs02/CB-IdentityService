using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Features.UserManagement.Services;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Results;
using IdentityServiceApi.Shared.Utilities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace IdentityServiceApi.UnitTests.Features.UserManagement
{
    [Trait("Category", "Unit")]
    public class UserLookupServiceTests
    {
        private readonly Mock<UserManager<User>> _userManager = UserManagerMock();
        private readonly Mock<IUserLookupResultFactory> _resultFactory = new();
        private readonly Mock<IParameterValidator> _validator = new();
        private readonly UserLookupService _service;

        public UserLookupServiceTests()
        {
            _service = new UserLookupService(
                _userManager.Object,
                _resultFactory.Object,
                _validator.Object
            );
        }

        [Fact]
        public async Task FindUserByIdAsync_UserNotFound_ReturnsFailure()
        {
            const string userId = "id";
            const string expectedError = ErrorMessages.User.NotFound;
            var expectedErrorType = ErrorType.NotFound;

            _userManager
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((User)null);

            _resultFactory
                .Setup(x => x.UserLookupOperationFailure(It.IsAny<string[]>(), expectedErrorType))
                .Returns(new UserLookupResult
                {
                    Success = false,
                    Errors = [expectedError],
                    ErrorType = expectedErrorType
                });

            var result = await _service.FindUserByIdAsync(userId);
            Assert.False(result.Success);

            _resultFactory.Verify(x =>
                x.UserLookupOperationFailure(It.IsAny<string[]>(), expectedErrorType),
                Times.Once);
        }

        [Fact]
        public async Task FindUserByIdAsync_UserFound_ReturnsSuccess()
        {
            const string userId = "id";
            var user = new User { Id = userId };

            _userManager
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _resultFactory
                .Setup(x => x.UserLookupOperationSuccess(user))
                .Returns(new UserLookupResult
                {
                    Success = true,
                    UserFound = user
                });

            var result = await _service.FindUserByIdAsync(userId);
            Assert.True(result.Success);
            Assert.Equal(user, result.UserFound);

            _resultFactory.Verify(x =>
                x.UserLookupOperationSuccess(user),
                Times.Once);
        }

        [Fact]
        public async Task FindUserByUsernameAsync_UserNotFound_ReturnsFailure()
        {
            const string username = "user";
            const string expectedError = ErrorMessages.User.NotFound;
            var expectedErrorType = ErrorType.NotFound;

            _userManager
                .Setup(x => x.FindByNameAsync(username))
                .ReturnsAsync((User)null);

            _resultFactory
                .Setup(x => x.UserLookupOperationFailure(It.IsAny<string[]>(), expectedErrorType))
                .Returns(new UserLookupResult
                {
                    Success = false,
                    Errors = [expectedError],
                    ErrorType = expectedErrorType
                });

            var result = await _service.FindUserByUsernameAsync(username);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task FindUserByUsernameAsync_UserFound_ReturnsSuccess()
        {
            const string username = "user";
            var user = new User { Id = "1", UserName = username };

            _userManager
                .Setup(x => x.FindByNameAsync(username))
                .ReturnsAsync(user);

            _resultFactory
                .Setup(x => x.UserLookupOperationSuccess(user))
                .Returns(new UserLookupResult
                {
                    Success = true,
                    UserFound = user
                });

            var result = await _service.FindUserByUsernameAsync(username);
            Assert.True(result.Success);
            Assert.Equal(username, result.UserFound.UserName);
        }

        private static Mock<UserManager<User>> UserManagerMock()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                store.Object,
                null!, null!, null!, null!, null!, null!, null!, null!);
        }
    }
}