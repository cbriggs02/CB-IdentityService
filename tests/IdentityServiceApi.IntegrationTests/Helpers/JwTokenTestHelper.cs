using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityServiceApi.IntegrationTests.Helpers
{
    /// <summary>
    ///     Helper class for generating JWT tokens for testing purposes.
    ///     This class uses an in-memory configuration to generate a token with customizable roles.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public static class JwtTokenTestHelper
    {
        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"JwtSettings:ValidIssuer", "https://localhost:52870"},
                {"JwtSettings:ValidAudience", "https://localhost:4200"},
                {"JwtSettings:SecretKey", "ED9591338BCE30B0738C2AF30CF40CD4F29399958ABD1501726609F3B1D4066B"}
            })
            .Build();


        /// <summary>
        ///     Generates a JWT token with a specified list of roles for testing.
        /// </summary>
        /// <param name="roles">
        ///     A list of roles to include in the generated JWT token. Can be null or empty.
        /// </param>
        /// <param name="userName">
        ///     The username of the user who the token is being issued for.
        /// </param>
        /// <param name="userId">
        ///     The id of the user who the token is being issued for.
        /// </param>
        /// <returns>
        ///     A JWT token string that can be used for authentication in integration tests.
        /// </returns>
        public static string GenerateJwtToken(IList<string> roles, string userName, string userId)
        {
            var validIssuer = Configuration["JwtSettings:ValidIssuer"];
            var validAudience = Configuration["JwtSettings:ValidAudience"];
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSettings:SecretKey"]));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, userName),
            };

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