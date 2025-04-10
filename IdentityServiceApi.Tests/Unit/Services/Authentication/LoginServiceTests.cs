using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.ServiceResultModels.UserManagement;
using IdentityServiceApi.Models.RequestModels.Authentication;
using IdentityServiceApi.Models.ServiceResultModels.Authentication;
using IdentityServiceApi.Services.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using IdentityServiceApi.Models.Configurations;
using System.Security.Cryptography;

namespace IdentityServiceApi.Tests.Unit.Services.Authentication
{
	/// <summary>
	///     Unit tests for the <see cref="LoginService"/> class.
	///     This class contains test cases for various login scenarios, verifying the 
	///     behavior of the login functionality.
	/// </summary>
	/// <remarks>
	///     @Author: Christian Briglio
	///     @Created: 2024
	/// </remarks>
	[Trait("TestCategory", "UnitTest")]
	public class LoginServiceTests
	{
		private readonly Mock<UserManager<User>> _userManagerMock;
		private readonly Mock<SignInManager<User>> _signInManagerMock;
		private readonly Mock<IOptions<JwtSettings>> _jwtSettingsOptionsMock;
		private readonly Mock<IParameterValidator> _parameterValidatorMock;
		private readonly Mock<ILoginServiceResultFactory> _loginServiceResultFactoryMock;
		private readonly Mock<IUserLookupService> _userLookupServiceMock;
		private readonly Mock<ILogger<UserManager<User>>> _userManagerLoggerMock;
		private readonly Mock<ILogger<SignInManager<User>>> _signInManagerLoggerMock;
		private readonly Mock<IUserStore<User>> _userStoreMock;
		private readonly Mock<IOptions<IdentityOptions>> _optionsMock;
		private readonly Mock<IPasswordHasher<User>> _userHasherMock;
		private readonly Mock<IUserValidator<User>> _userValidatorMock;
		private readonly Mock<IPasswordValidator<User>> _passwordValidatorsMock;
		private readonly Mock<ILookupNormalizer> _keyNormalizerMock;
		private readonly Mock<IdentityErrorDescriber> _errorsMock;
		private readonly Mock<IServiceProvider> _serviceProviderMock;
		private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
		private readonly Mock<IUserClaimsPrincipalFactory<User>> _claimsFactoryMock;
		private readonly Mock<IAuthenticationSchemeProvider> _schemesMock;
		private readonly Mock<IUserConfirmation<User>> _confirmationMock;
		private readonly LoginService _loginService;
		private readonly JwtSettings _jwtSettings;
		private const string TestPassword = "test-password";

		/// <summary>
		///     Initializes a new instance of the <see cref="LoginServiceTests"/> class.
		///     This constructor sets up the mocked dependencies and creates an instance 
		///     of the <see cref="LoginService"/> for testing.
		/// </summary>
		public LoginServiceTests()
		{
			_userStoreMock = new Mock<IUserStore<User>>();
			_optionsMock = new Mock<IOptions<IdentityOptions>>();
			_userHasherMock = new Mock<IPasswordHasher<User>>();
			_userValidatorMock = new Mock<IUserValidator<User>>();
			_passwordValidatorsMock = new Mock<IPasswordValidator<User>>();
			_keyNormalizerMock = new Mock<ILookupNormalizer>();
			_errorsMock = new Mock<IdentityErrorDescriber>();
			_serviceProviderMock = new Mock<IServiceProvider>();
			_userManagerLoggerMock = new Mock<ILogger<UserManager<User>>>();

			_signInManagerLoggerMock = new Mock<ILogger<SignInManager<User>>>();
			_httpContextAccessorMock = new Mock<IHttpContextAccessor>();
			_claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
			_schemesMock = new Mock<IAuthenticationSchemeProvider>();
			_confirmationMock = new Mock<IUserConfirmation<User>>();

			_optionsMock.Setup(o => o.Value).Returns(new IdentityOptions());

			_userManagerMock = new Mock<UserManager<User>>(
				_userStoreMock.Object,
				_optionsMock.Object,
				_userHasherMock.Object,
				new[] { _userValidatorMock.Object },
				new[] { _passwordValidatorsMock.Object },
				_keyNormalizerMock.Object,
				_errorsMock.Object,
				_serviceProviderMock.Object,
				_userManagerLoggerMock.Object
			);

			_signInManagerMock = new Mock<SignInManager<User>>(
				_userManagerMock.Object,
				_httpContextAccessorMock.Object,
				_claimsFactoryMock.Object,
				_optionsMock.Object,
				_signInManagerLoggerMock.Object,
				_schemesMock.Object,
				_confirmationMock.Object
			);

			_parameterValidatorMock = new Mock<IParameterValidator>();
			_loginServiceResultFactoryMock = new Mock<ILoginServiceResultFactory>();
			_userLookupServiceMock = new Mock<IUserLookupService>();
			_jwtSettingsOptionsMock = new Mock<IOptions<JwtSettings>>();

			var keyBytes = RandomNumberGenerator.GetBytes(32);
			var keyBase64 = Convert.ToBase64String(keyBytes);

			_jwtSettings = new JwtSettings
			{
				ValidIssuer = "Issuer test",
				ValidAudience = "Audience test",
				SecretKey = keyBase64
			};

			_jwtSettingsOptionsMock
				.Setup(options => options.Value)
				.Returns(_jwtSettings);

			_loginService = new LoginService(_signInManagerMock.Object, _userManagerMock.Object, _jwtSettingsOptionsMock.Object, _loginServiceResultFactoryMock.Object, _parameterValidatorMock.Object, _userLookupServiceMock.Object);
		}

		/// <summary>
		///     Tests that an <see cref="ArgumentNullException"/> is thrown when <see cref="LoginService"/> is 
		///     instantiated with a null dependencies.
		/// </summary>
		[Fact]
		public void LoginService_NullDependencies_ThrowsArgumentNullException()
		{
			//Act & Assert
			Assert.Throws<ArgumentNullException>(() => new LoginService(null, null, null, null, null, null));
		}

		/// <summary>
		///     Verifies that calling the <see cref="LoginService.LoginAsync(LoginRequest)"/> with null credentials throws an ArgumentNullException.
		/// </summary>
		/// <returns>
		///     A task representing the asynchronous test operation.
		/// </returns>
		[Fact]
		public async Task Login_NullCredentialsObject_ThrowsArgumentNullException()
		{
			// Arrange
			_parameterValidatorMock
				.Setup(x => x.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()))
				.Throws<ArgumentNullException>();

			// Act & Assert
			await Assert.ThrowsAsync<ArgumentNullException>(() => _loginService.LoginAsync(null));

			VerifyCallsToParameterService(0);
		}

		/// <summary>
		///     Verifies that calling the <see cref="LoginService.LoginAsync(LoginRequest)"/> with null, empty or whitespace username 
		///     or password throws an ArgumentNullException.
		/// </summary>
		/// <param name="input">
		///     Used to test for invalid data like ( null, empty or whitespace )
		/// </param>
		/// <returns>
		///     A task representing the asynchronous test operation.
		/// </returns>
		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		public async Task Login_InvalidCredentials_ThrowsArgumentNullException(string input)
		{
			// Arrange
			var request = new LoginRequest { UserName = input, Password = input };

			_parameterValidatorMock
				.Setup(x => x.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
				.Throws<ArgumentNullException>();

			// Act & Assert
			await Assert.ThrowsAsync<ArgumentNullException>(() => _loginService.LoginAsync(request));

			VerifyCallsToParameterService(1);
		}

		/// <summary>
		///     Tests the login functionality when the user is not found.
		///     Expects a result indicating the user was not found.
		/// </summary>
		/// <returns>
		///     A task representing the asynchronous operation.
		/// </returns>
		[Fact]
		public async Task Login_UserNotFound_ReturnsNotFoundResult()
		{
			// Arrange
			const string nonExistentUserName = "nonexistent";
			const string expectedErrorMessage = ErrorMessages.User.NotFound;

			var request = new LoginRequest { UserName = nonExistentUserName, Password = TestPassword };

			_userLookupServiceMock
				.Setup(x => x.FindUserByUsernameAsync(nonExistentUserName))
				.ReturnsAsync(new UserLookupServiceResult
				{
					Success = false,
					Errors = new[] { expectedErrorMessage }.ToList()
				});

			ArrangeServiceResult(expectedErrorMessage);

			// Act
			var result = await _loginService.LoginAsync(request);

			// Assert
			Assert.NotNull(result);
			Assert.False(result.Success);
			Assert.Contains(expectedErrorMessage, result.Errors);

			VerifyCallsToLookupService(nonExistentUserName);
			VerifyCallsToParameterService(2);
		}

		/// <summary>
		///     Tests the login functionality when the user is not activated.
		///     Expects a result indicating the user is not activated.
		/// </summary>
		/// <returns>
		///     A task representing the asynchronous operation.
		/// </returns>
		[Fact]
		public async Task Login_UserNotActivated_ReturnsNotActivatedResult()
		{
			// Arrange
			const string expectedErrorMessage = ErrorMessages.User.NotActivated;
			var inactiveUser = CreateMockUser(false);

			ArrangeUserLookupResult(inactiveUser);
			ArrangeServiceResult(expectedErrorMessage);

			var request = CreateRequestObject(TestPassword, inactiveUser);

			// Act
			var result = await _loginService.LoginAsync(request);

			// Assert
			Assert.NotNull(result);
			Assert.False(result.Success);
			Assert.Contains(expectedErrorMessage, result.Errors);

			VerifyCallsToLookupService(inactiveUser.UserName);
			VerifyCallsToParameterService(2);
		}

		/// <summary>
		///     Tests the login functionality with invalid credentials.
		///     Expects a result indicating invalid credentials were provided.
		/// </summary>
		/// <returns>
		///     A task representing the asynchronous operation.
		/// </returns>
		[Fact]
		public async Task Login_WrongPassword_ReturnsInvalidCredentialsResult()
		{
			// Arrange
			const string wrongPassword = "wrong-password";
			const string expectedErrorMessage = ErrorMessages.Password.InvalidCredentials;

			var user = CreateMockUser(true);

			ArrangeUserLookupResult(user);

			_signInManagerMock
				.Setup(j => j.PasswordSignInAsync(user, wrongPassword, false, true))
				.ReturnsAsync(SignInResult.Failed);

			ArrangeServiceResult(expectedErrorMessage);

			var request = CreateRequestObject(wrongPassword, user);

			// Act
			var result = await _loginService.LoginAsync(request);

			// Assert
			Assert.NotNull(result);
			Assert.False(result.Success);
			Assert.Contains(expectedErrorMessage, result.Errors);

			VerifyCallsToLookupService(user.UserName);
			VerifyCallsToParameterService(2);
		}

		/// <summary>
		///     Verifies that the <see cref="LoginService.LoginAsync"/> method returns a failure result when 
		///     <see cref="SignInManager.PasswordSignInAsync"/> fails due to invalid credentials. 
		///     The method checks that the appropriate error message is returned indicating invalid credentials.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation, with the result being 
		///     a failure containing the specified error message for invalid credentials.
		/// </returns>
		[Fact]
		public async Task Login_PasswordSignInAsyncFails_ReturnsLoginOperationFailureResult()
		{
			// Arrange
			const string expectedErrorMessage = ErrorMessages.Password.InvalidCredentials;
			const string correctPassword = "correct-password";

			var mockUser = CreateMockUser(true);

			ArrangeUserLookupResult(mockUser);

			_signInManagerMock
				.Setup(j => j.PasswordSignInAsync(mockUser, correctPassword, false, true))
				 .ReturnsAsync(SignInResult.Failed);

			ArrangeServiceResult(expectedErrorMessage);

			var request = CreateRequestObject(correctPassword, mockUser);

			// Act
			var result = await _loginService.LoginAsync(request);

			// Assert
			Assert.NotNull(result);
			Assert.False(result.Success);
			Assert.Contains(expectedErrorMessage, result.Errors);

			_signInManagerMock.Verify(s => s.PasswordSignInAsync(mockUser, correctPassword, false, true), Times.Once);

			VerifyCallsToLookupService(mockUser.UserName);
			VerifyCallsToParameterService(2);
		}

		/// <summary>
		///     Tests the login functionality for a successful login.
		///     Expects a result indicating success and a valid JWT token.
		/// </summary>
		/// <returns>
		///     A task representing the asynchronous operation.
		/// </returns>
		[Fact]
		public async Task Login_SuccessfulLogin_ReturnsToken()
		{
			// Arrange
			const string correctPassword = "correct-password";

			var mockUser = CreateMockUser(true);

			ArrangeUserLookupResult(mockUser);

			_signInManagerMock
				.Setup(j => j.PasswordSignInAsync(mockUser, correctPassword, false, true))
				.ReturnsAsync(SignInResult.Success);

			var expectedResult = new LoginServiceResult
			{
				Success = true,
				Token = "token" // Mocked placeholder token
			};

			_loginServiceResultFactoryMock
				.Setup(x => x.LoginOperationSuccess(It.IsAny<string>()))
				.Returns(expectedResult);

			var request = CreateRequestObject(correctPassword, mockUser);

			// Act
			var result = await _loginService.LoginAsync(request);

			// Assert
			Assert.NotNull(result);
			Assert.True(expectedResult.Success);
			Assert.NotNull(expectedResult.Token);

			_signInManagerMock.Verify(s => s.PasswordSignInAsync(mockUser, correctPassword, false, true), Times.Once);

			VerifyCallsToLookupService(mockUser.UserName);
			VerifyCallsToParameterService(2);
		}

		private static LoginRequest CreateRequestObject(string password, User user)
		{
			return new LoginRequest { UserName = user.UserName, Password = password };
		}

		private static User CreateMockUser(bool accountStatus)
		{
			const string mockUserName = "test-user";
			return new User { UserName = mockUserName, AccountStatus = accountStatus ? 1 : 0 };
		}

		private void ArrangeServiceResult(string expectedErrorMessage)
		{
			var result = new LoginServiceResult
			{
				Success = false,
				Errors = new List<string> { expectedErrorMessage }
			};

			_loginServiceResultFactoryMock
				.Setup(x => x.LoginOperationFailure(new[] { expectedErrorMessage }))
				.Returns(result);
		}

		private void ArrangeUserLookupResult(User user)
		{
			var result = new UserLookupServiceResult
			{
				Success = true,
				UserFound = user,
			};

			_userLookupServiceMock
				.Setup(x => x.FindUserByUsernameAsync(user.UserName))
				.ReturnsAsync(result);
		}

		private void VerifyCallsToParameterService(int numberOfTimes)
		{
			_parameterValidatorMock.Verify(v => v.ValidateObjectNotNull(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
			_parameterValidatorMock.Verify(v => v.ValidateNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(numberOfTimes));
		}

		private void VerifyCallsToLookupService(string username)
		{
			_userLookupServiceMock.Verify(l => l.FindUserByUsernameAsync(username), Times.Once);
		}
	}
}
