using IdentityServiceApi.Models.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace IdentityServiceApi.Extensions
{
    /// <summary>
    ///     Provides extension methods to configure JWT authentication services for the IdentityServiceApi.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public static class AuthenticationExtensions
    {
        /// <summary>
        ///     Adds JWT authentication to the service collection using settings from the application's configuration.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>
        ///     to which the authentication services will be added.
        /// </param>
        /// <param name="configuration">
        ///     The <see cref="IConfiguration"/> instance used to retrieve JWT settings.
        /// </param>
        /// <returns>
        ///     The same <see cref="IServiceCollection"/> instance to allow method chaining.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the <c>JwtSettings</c> section is missing or if the secret key is less than 32 bytes in length.
        /// </exception>
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
                ?? throw new InvalidOperationException("JwtSettings configuration section is missing.");

            var secretKeyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
            if (secretKeyBytes.Length < 32)
            {
                throw new InvalidOperationException("JwtSettings.SecretKey must be at least 32 bytes (256 bits) for HS256 signing.");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ValidIssuer = jwtSettings.ValidIssuer,
                    ValidAudience = jwtSettings.ValidAudience
                };
            });

            return services;
        }
    }
}
