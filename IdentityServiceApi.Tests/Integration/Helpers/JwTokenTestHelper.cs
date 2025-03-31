using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityServiceApi.Tests.Integration.Helpers
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
                {"JwtSettings:ValidIssuer", "https://localhost:7234"},
                {"JwtSettings:ValidAudience", "https://localhost:3000"},
                {"JwtSettings:SecretKey", "NjwQN0JTxC1^Rd5VEFf&&@e$$q4B0BaR^y8Q%1t&!FHo)a%@w&1pi)WJKjbaMVlk"}
            })
            .Build();

        /// <summary>
        ///     Generates a JWT token with a specified list of roles for testing.
        /// </summary>
        /// <param name="roles">
        ///     A list of roles to include in the generated JWT token. Can be null or empty.
        /// </param>
        /// <returns>
        ///     A JWT token string that can be used for authentication in integration tests.
        /// </returns>
        public static string GenerateJwtToken(IList<string> roles)
        {
            const string userId = "test-user-id";
            const string userName = "test-user";

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
