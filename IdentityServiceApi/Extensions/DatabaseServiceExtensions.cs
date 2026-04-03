using IdentityServiceApi.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityServiceApi.Extensions
{
    /// <summary>
    ///     Provides extension methods for configuring database services for the application.
    ///     This class contains methods to configure the application's database context.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public static class DatabaseServiceExtensions
    {
        /// <summary>
        ///     Configures the database context for the application.
        ///     It sets up the application's main database connection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>
        ///     used to register services for dependency injection.
        /// </param>
        /// <param name="configuration">
        ///     The <see cref="IConfiguration"/> containing the application's settings, including connection strings.
        /// </param>
        /// <param name="environment">
        ///     The <see cref="IWebHostEnvironment"/> that indicates the hosting environment (e.g., Development, Staging, Production).
        /// </param>
        /// <returns>
        ///     The updated <see cref="IServiceCollection"/> with the database services configured.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if a required connection string "ApplicationDatabase" is missing
        ///     from the configuration.
        /// </exception>
        public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            var applicationDatabaseConnectionString = configuration.GetConnectionString("ApplicationDatabase")
                ?? throw new InvalidOperationException("ApplicationDatabase connection string is missing in the configuration.");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseLazyLoadingProxies()
                    .UseSqlServer(applicationDatabaseConnectionString);
            });

            return services;
        }
    }
}
