using IdentityServiceApi.Constants;
using IdentityServiceApi.Enums;
using IdentityServiceApi.Interfaces.Authentication;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Configurations;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.RequestModels.Authentication;
using IdentityServiceApi.Models.ServiceResultModels.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityServiceApi.Services.Authentication
{
    /// <summary>
    ///     Service responsible for interacting with login-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class LoginService(SignInManager<User> signInManager, UserManager<User> userManager, IOptions<JwtSettings> jwtSettings, ILoginServiceResultFactory loginServiceResultFactory, IParameterValidator parameterValidator, IUserLookupService userLookupService, ILoggerService loggerService) : ILoginService
    {
        private readonly SignInManager<User> _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        private readonly UserManager<User> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly JwtSettings _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        private readonly ILoginServiceResultFactory _loginServiceResultFactory = loginServiceResultFactory ?? throw new ArgumentNullException(nameof(loginServiceResultFactory));
        private readonly IParameterValidator _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
        private readonly IUserLookupService _userLookupService = userLookupService ?? throw new ArgumentNullException(nameof(userLookupService));
        private readonly ILoggerService _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));

        /// <summary>
        ///     Asynchronously logs in a user in the system using the sign-in manager based on provided credentials.
        /// </summary>
        /// <param name="credentials">
        ///     Required information used to authenticate the user during login.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="LoginServiceResult"/> indicating the login status.   
        ///     - If successful, returns a <see cref="LoginServiceResult"/> with Success set to true.    
        ///     - If the provided username could not be located, returns an error message.
        ///     - If the provided password does not match the located user, returns an error message.  
        ///     - If an error occurs during login, returns <see cref="LoginServiceResult"/> with an error message.
        /// </returns>
        public async Task<LoginServiceResult> LoginAsync(LoginRequest credentials)
        {
            _parameterValidator.ValidateObjectNotNull(credentials, nameof(credentials));
            _parameterValidator.ValidateNotNullOrEmpty(credentials.UserName, nameof(credentials.UserName));
            _parameterValidator.ValidateNotNullOrEmpty(credentials.Password, nameof(credentials.Password));

            var userLookupResult = await _userLookupService.FindUserByUsernameAsync(credentials.UserName);
            if (!userLookupResult.Success)
            {
                return _loginServiceResultFactory.LoginOperationFailure([.. userLookupResult.Errors]);
            }

            var user = userLookupResult.UserFound;

            if (user.AccountStatus != 1)
            {
                return _loginServiceResultFactory.LoginOperationFailure([ErrorMessages.User.NotActivated]);
            }

            var result = await _signInManager.PasswordSignInAsync(user, credentials.Password, false, true);
            if (!result.Succeeded)
            {
                _loggerService.LogData(LogLevel.Warning, LogSource.LoginService, $"Failed login attempt for user {credentials.UserName}");
                return _loginServiceResultFactory.LoginOperationFailure([ErrorMessages.Password.InvalidCredentials]);
            }

            var token = await GenerateJwtTokenAsync(user);
            return _loginServiceResultFactory.LoginOperationSuccess(token);
        }

        /// <summary>
        ///     Generates a JWT token for the specified user based on configured settings.
        /// </summary>
        /// <param name="user">
        ///     The user for whom the token is generated.
        /// </param>
        /// <returns>
        ///     A string representing the generated JWT token.
        /// </returns>
        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            var validIssuer = _jwtSettings.ValidIssuer;
            var validAudience = _jwtSettings.ValidAudience;

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
            };

            if (!string.IsNullOrWhiteSpace(user.UserName))
            {
                claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles != null && roles.Any())
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var tokenOptions = new JwtSecurityToken(
                issuer: validIssuer,
                audience: validAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: signingCredentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            return tokenString;
        }
    }
}
