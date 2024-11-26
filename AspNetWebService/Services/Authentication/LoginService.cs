﻿using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Authentication;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.RequestModels.Authentication;
using IdentityServiceApi.Models.ServiceResultModels.Authentication;
using Microsoft.AspNetCore.Identity;
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
    /// </remarks>
    public class LoginService : ILoginService
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IParameterValidator _parameterValidator;
        private readonly IServiceResultFactory _serviceResultFactory;
        private readonly IUserLookupService _userLookupService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LoginService"/> class.
        /// </summary>
        /// <param name="signInManager">
        ///     The sign-in manager used for user authentication.
        /// </param>
        /// <param name="userManager">
        ///     The user manager responsible for handling user management operations.
        /// </param>
        /// <param name="configuration">
        ///     The configuration used for accessing app settings, including JWT settings.
        /// </param>
        /// <param name="parameterValidator">
        ///     The parameter validator service used for defense checking service parameters.
        /// </param>
        /// <param name="serviceResultFactory">
        ///     The service used for creating the result objects being returned in operations.
        /// </param>
        /// <param name="userLookupService">'
        ///     The service used for looking up users in the system.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public LoginService(SignInManager<User> signInManager, UserManager<User> userManager, IConfiguration configuration, IParameterValidator parameterValidator, IServiceResultFactory serviceResultFactory, IUserLookupService userLookupService)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
            _serviceResultFactory = serviceResultFactory ?? throw new ArgumentNullException(nameof(serviceResultFactory));
            _userLookupService = userLookupService ?? throw new ArgumentNullException(nameof(userLookupService));
        }


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
        public async Task<LoginServiceResult> Login(LoginRequest credentials)
        {
            _parameterValidator.ValidateObjectNotNull(credentials, nameof(credentials));
            _parameterValidator.ValidateNotNullOrEmpty(credentials.UserName, nameof(credentials.UserName));
            _parameterValidator.ValidateNotNullOrEmpty(credentials.Password, nameof(credentials.Password));

            var userLookupResult = await _userLookupService.FindUserByUsername(credentials.UserName);

            if (!userLookupResult.Success)
            {
                return _serviceResultFactory.LoginOperationFailure(userLookupResult.Errors.ToArray());
            }

            var user = userLookupResult.UserFound;

            if (user.AccountStatus == 0)
            {
                return _serviceResultFactory.LoginOperationFailure(new[] { ErrorMessages.User.NotActivated });
            }
            
            var result = await _signInManager.PasswordSignInAsync(user, credentials.Password, false, true);

            if (!result.Succeeded)
            {
                return _serviceResultFactory.LoginOperationFailure(new[] { ErrorMessages.Password.InvalidCredentials });
            }

            var token = await GenerateJwtToken(user);
            return _serviceResultFactory.LoginOperationSuccess(token);
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
        private async Task<string> GenerateJwtToken(User user)
        {
            var validIssuer = _configuration["JwtSettings:ValidIssuer"];
            var validAudience = _configuration["JwtSettings:ValidAudience"];

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName),
            };

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
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: signingCredentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            return tokenString;
        }
    }
}
