using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Requests;
using IdentityServiceApi.Features.UserManagement.Models.Results;
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
        public class SetPasswordAsyncTests : PasswordServiceTests
        {
            [Fact]
            public async Task SetPasswordAsync_Mismatch_ReturnsFailure()
            {
                var expectedErrorType = ErrorType.Validation;
                var expected = new Result
                {
                    Success = false,
                    Errors = [ErrorMessages.Password.Mismatch],
                    ErrorType = expectedErrorType
                };

                _resultFactory
                    .Setup(x => x.GeneralOperationFailure(It.IsAny<string[]>(), expectedErrorType))
                    .Returns(expected);

                var request = new SetPasswordRequest
                {
                    Password = "A",
                    PasswordConfirmed = "B"
                };

                var result = await _service.SetPasswordAsync("id", request);
                Assert.False(result.Success);
            }

            [Fact]
            public async Task SetPasswordAsync_UserNotFound_ReturnsFailure()
            {
                const string expectedError = ErrorMessages.User.NotFound;
                var expectedErrorType = ErrorType.NotFound;

                _userLookup
                    .Setup(x => x.FindUserByIdAsync("id"))
                    .ReturnsAsync(new UserLookupResult
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

                var result = await PrepareSetPassword("id");
                Assert.False(result.Success);
            }

            [Fact]
            public async Task SetPasswordAsync_AlreadySet_ReturnsFailure()
            {
                const string expectedError = ErrorMessages.Password.AlreadySet;
                var expectedErrorType = ErrorType.InvalidState;
                var user = new User { Id = "id", PasswordHash = "hash" };

                _userLookup
                    .Setup(x => x.FindUserByIdAsync("id"))
                    .ReturnsAsync(new UserLookupResult
                    {
                        Success = true,
                        UserFound = user
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

                var result = await PrepareSetPassword("id");
                Assert.False(result.Success);
            }

            [Fact]
            public async Task SetPasswordAsync_Success_CreatesHistoryAndLogs()
            {
                var user = new User { Id = "id" };

                _userLookup
                    .Setup(x => x.FindUserByIdAsync("id"))
                    .ReturnsAsync(new UserLookupResult
                    {
                        Success = true,
                        UserFound = user
                    });

                _userManager
                    .Setup(x => x.AddPasswordAsync(user, "pass"))
                    .ReturnsAsync(IdentityResult.Success)
                    .Callback<User, string>((u, p) => u.PasswordHash = "hashed-pass");

                _resultFactory
                    .Setup(x => x.GeneralOperationSuccess())
                    .Returns(new Result { Success = true });

                var result = await PrepareSetPassword("id");
                Assert.True(result.Success);

                _history.Verify(x =>
                    x.AddPasswordHistoryAsync(It.IsAny<StorePasswordHistoryRequest>()),
                    Times.Once);

                _logger.Verify(x =>
                    x.LogData(It.Is<LogEntry>(l =>
                        l.LogLevel == LogLevel.Information &&
                        l.Message.Contains("Password set"))),
                    Times.Once);
            }
        }
    }
}
