using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Requests;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Shared.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace IdentityServiceApi.UnitTests.Features.UserManagement.PasswordServiceTests
{
    public partial class PasswordServiceTests
    {
        public class UpdatePasswordAsyncTests : PasswordServiceTests
        {
            [Fact]
            public async Task UpdatePasswordAsync_PermissionFail_ReturnsFailure()
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

                var expected = new Result
                {
                    Success = false,
                    Errors = [expectedError],
                    ErrorType = expectedErrorType
                };

                _resultFactory
                    .Setup(x => x.GeneralOperationFailure(It.IsAny<string[]>(), expectedErrorType))
                    .Returns(expected);

                var result = await PrepareUpdatePassword("id");
                Assert.False(result.Success);
            }

            [Fact]
            public async Task UpdatePasswordAsync_InvalidPassword_ReturnsFailure()
            {
                const string expectedError = ErrorMessages.Password.InvalidCredentials;
                var expectedErrorType = ErrorType.Unauthorized;
                var user = new User { Id = "id", PasswordHash = "hash" };
                SetupValidPermissionAndUser(user);

                _userManager
                    .Setup(x => x.CheckPasswordAsync(user, "old"))
                    .ReturnsAsync(false);

                var expected = new Result
                {
                    Success = false,
                    Errors = [expectedError],
                    ErrorType = expectedErrorType
                };

                _resultFactory
                    .Setup(x => x.GeneralOperationFailure(It.IsAny<string[]>(), expectedErrorType))
                    .Returns(expected);

                var result = await PrepareUpdatePassword("id");
                Assert.False(result.Success);
            }

            [Fact]
            public async Task UpdatePasswordAsync_ReusedPassword_ReturnsFailure()
            {
                const string expectedError = ErrorMessages.Password.CannotReuse;
                var expectedErrorType = ErrorType.Validation;
                var user = new User { Id = "id", PasswordHash = "hash" };
                SetupValidPermissionAndUser(user);

                _userManager
                    .Setup(x => x.CheckPasswordAsync(user, "old"))
                    .ReturnsAsync(true);

                _history
                    .Setup(x => x.FindPasswordHashAsync(It.IsAny<SearchPasswordHistoryRequest>()))
                    .ReturnsAsync(true);

                var expected = new Result
                {
                    Success = false,
                    Errors = [expectedError],
                    ErrorType = expectedErrorType
                };

                _resultFactory
                    .Setup(x => x.GeneralOperationFailure(It.IsAny<string[]>(), expectedErrorType))
                    .Returns(expected);

                var result = await PrepareUpdatePassword("id");
                Assert.False(result.Success);
            }

            [Fact]
            public async Task UpdatePasswordAsync_Success_UpdatesAndCreatesHistory()
            {
                var user = new User { Id = "id", PasswordHash = "hash" };
                SetupValidPermissionAndUser(user);

                _userManager
                    .Setup(x => x.CheckPasswordAsync(user, "old"))
                    .ReturnsAsync(true);

                _history
                    .Setup(x => x.FindPasswordHashAsync(It.IsAny<SearchPasswordHistoryRequest>()))
                    .ReturnsAsync(false);

                _userManager
                    .Setup(x => x.ChangePasswordAsync(user, "old", "new"))
                    .ReturnsAsync(IdentityResult.Success);

                _resultFactory
                    .Setup(x => x.GeneralOperationSuccess())
                    .Returns(new Result { Success = true });

                var result = await PrepareUpdatePassword("id");
                Assert.True(result.Success);

                _history.Verify(x =>
                    x.AddPasswordHistoryAsync(It.IsAny<StorePasswordHistoryRequest>()),
                    Times.Once);

                _logger.Verify(x =>
                    x.LogData(It.Is<LogEntry>(l =>
                        l.LogLevel == LogLevel.Information &&
                        l.Message.Contains("Password changed"))),
                    Times.Once);
            }
        }
    }
}
