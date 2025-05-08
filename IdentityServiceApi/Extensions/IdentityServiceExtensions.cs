using IdentityServiceApi.Data;
using IdentityServiceApi.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentityServiceApi.Extensions
{
    /// <summary>
    ///     Provides extension methods for configuring identity services, such as user authentication and authorization.
    ///     This class contains methods to configure ASP.NET Identity for user management.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public static class IdentityServiceExtensions
    {
        /// <summary>
        ///     Configures the identity services for user authentication and authorization.
        ///     This method sets up ASP.NET Identity with customized password policies, lockout settings, 
        ///     and unique email requirement. It also configures the password hasher for compatibility with IdentityV3.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="IServiceCollection"/> used to register services for dependency injection.
        /// </param>
        /// <returns>
        ///     The updated <see cref="IServiceCollection"/> with identity services configured.
        /// </returns>
        public static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.Configure<PasswordHasherOptions>(options =>
            {
                options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
            });

            return services;
        }
    }
}
