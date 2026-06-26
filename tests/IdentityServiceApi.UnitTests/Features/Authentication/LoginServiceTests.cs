using IdentityServiceApi.Features.Authentication.Interfaces;
using IdentityServiceApi.Features.Authentication.Models;
using IdentityServiceApi.Features.Authentication.Services;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Shared.Results;
using IdentityServiceApi.Shared.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IdentityServiceApi.UnitTests.Features.Authentication
{
    [Trait("Category", "Unit")]
    public class LoginServiceTests
    {
        private readonly Mock<SignInManager<User>> _signInManager = SignInManagerMock();
        private readonly Mock<UserManager<User>> _userManager = UserManagerMock();
        private readonly Mock<ILoginResultFactory> _resultFactory = new();
        private readonly Mock<IParameterValidator> _validator = new();
        private readonly Mock<IUserLookupService> _userLookup = new();
        private readonly Mock<ILoggerService> _logger = new();
        private readonly LoginService _service;

        public LoginServiceTests()
        {
            var jwt = Options.Create(new JwtSettings
            {
                ValidIssuer = "issuer",
                ValidAudience = "audience",
                SecretKey = "THIS_IS_A_SUPER_SECRET_KEY_12345"
            });

            _service = new LoginService(_signInManager.Object, _userManager.Object, jwt, _resultFactory.Object, _validator.Object, _userLookup.Object, _logger.Object);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ReturnsFailure()
        {
            const string expectedErrorMessage = ErrorMessages.User.NotFound;
            var expectedErrorType = ErrorType.NotFound;
            var request = new LoginRequest { UserName = "user", Password = "pass" };

            _userLookup
                .Setup(x => x.FindUserByUsernameAsync(request.UserName))
                .ReturnsAsync(new UserLookupResult
                {
                    Success = false,
                    Errors = [expectedErrorMessage],
                    ErrorType = expectedErrorType
                });

            var expected = new LoginResult
            {
                Success = false,
                Errors = [expectedErrorMessage],
                ErrorType = expectedErrorType
            };

            _resultFactory
                .Setup(x => x.LoginOperationFailure(It.IsAny<string[]>(), expectedErrorType))
                .Returns(expected);

            LoginResult result = await _service.LoginAsync(request);
            Assert.False(result.Success);

            _resultFactory.Verify(x =>
                x.LoginOperationFailure(It.IsAny<string[]>(), expectedErrorType),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_UserInactive_ReturnsFailure()
        {
            const string expectedErrorMessage = ErrorMessages.User.NotActivated;
            var expectedErrorType = ErrorType.InvalidState;
            var user = new User { Id = "1", UserName = "user", AccountStatus = 0 };

            _userLookup
                .Setup(x => x.FindUserByUsernameAsync(user.UserName))
                .ReturnsAsync(new UserLookupResult
                {
                    Success = true,
                    UserFound = user
                });

            var expected = new LoginResult
            {
                Success = false,
                Errors = [expectedErrorMessage],
                ErrorType = expectedErrorType
            };

            _resultFactory
                .Setup(x => x.LoginOperationFailure(It.IsAny<string[]>(), expectedErrorType))
                .Returns(expected);

            LoginResult result = await PrepareLoginResult(user);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_LogsAndReturnsFailure()
        {
            const string expectedErrorMessage = ErrorMessages.Password.InvalidCredentials;
            var expectedErrorType = ErrorType.Unauthorized;
            var user = new User { Id = "1", UserName = "user", AccountStatus = 1 };

            _userLookup
                 .Setup(x => x.FindUserByUsernameAsync(user.UserName))
                 .ReturnsAsync(new UserLookupResult
                 {
                     Success = true,
                     UserFound = user
                 });

            _signInManager
                .Setup(x => x.PasswordSignInAsync(user, It.IsAny<string>(), false, true))
                .ReturnsAsync(SignInResult.Failed);

            var expected = new LoginResult
            {
                Success = false,
                Errors = [expectedErrorMessage],
                ErrorType = expectedErrorType
            };

            _resultFactory
                .Setup(x => x.LoginOperationFailure(It.IsAny<string[]>(), expectedErrorType))
                .Returns(expected);

            LoginResult result = await PrepareLoginResult(user);
            Assert.False(result.Success);

            _logger.Verify(x =>
                x.LogData(It.Is<LogEntry>(l =>
                    l.Message.Contains("Failed login attempt") &&
                    l.LogLevel == LogLevel.Warning)),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_Success_ReturnsToken()
        {
            var user = new User { Id = "1", UserName = "user", AccountStatus = 1 };

            _userLookup
               .Setup(x => x.FindUserByUsernameAsync(user.UserName))
               .ReturnsAsync(new UserLookupResult
               {
                   Success = true,
                   UserFound = user
               });

            _signInManager
                .Setup(x => x.PasswordSignInAsync(user, "pass", false, true))
                .ReturnsAsync(SignInResult.Success);

            _userManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync([Roles.Admin]);

            _resultFactory
                .Setup(x => x.LoginOperationSuccess(It.IsAny<string>()))
                .Returns<string>(token => new LoginResult
                {
                    Success = true,
                    Token = token
                });

            LoginResult result = await PrepareLoginResult(user);
            Assert.True(result.Success);
            Assert.NotNull(result.Token);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.Token);

            Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == "1");
            Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Name && c.Value == "user");
            Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }

        private async Task<LoginResult> PrepareLoginResult(User user)
        {
            return await _service.LoginAsync(new LoginRequest
            {
                UserName = user.UserName,
                Password = "pass"
            });
        }

        private static Mock<UserManager<User>> UserManagerMock()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                store.Object,
                null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private static Mock<SignInManager<User>> SignInManagerMock()
        {
            var userManager = UserManagerMock().Object;
            return new Mock<SignInManager<User>>(
                userManager,
                new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<User>>().Object,
                null!, null!, null!, null!);
        }
    }
}

