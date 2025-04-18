# IdentityServiceApi.Tests

This project contains unit and integration tests for the **IdentityServiceApi**. It validates authentication, authorization, user management, and role-based access control functionality.

---

## Folder Structure

```
IdentityServiceApi.Tests/
├── Integration/
│   ├── Helpers/
│   │   └── JwtTokenTestHelper.cs
│   └── Controllers/
|   └── Middleware/
├── Unit/
|   └── Helpers/
│   └── Services/
└── README.md
```

---

## Integration Testing Setup

Integration tests require authentication tokens to test protected endpoints. To support this, a custom helper generates mock JWT tokens used during test runs.

### JWT Token Generation Helper

A helper class named `JwtTokenTestHelper` is used to generate valid JWT tokens for test authentication. **This file is not currently included in the repository** for security reasons, as it contains a hardcoded secret key.

> **You must manually create this file before running integration tests.**

---

## Creating the `JwtTokenTestHelper.cs`

**Path:** `IdentityServiceApi.Tests/Integration/Helpers/JwtTokenTestHelper.cs`

Paste the following code into the new file:

```csharp
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
                {"JwtSettings:ValidIssuer", "https://localhost:52870"},
                {"JwtSettings:ValidAudience", "https://localhost:3000"},
                {"JwtSettings:SecretKey", "{replace with your secret key}"}
            })
            .Build();

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

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }
    }
}
```

---

## Notes

- Never commit hardcoded secrets to version control. This file is ignored by `.gitignore` and must be manually created on each environment.
- You may use environment variables or secret management tools (like [dotnet user-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets/)) for more secure test configurations.

---

##  Author

Christian Briglio – 2025
