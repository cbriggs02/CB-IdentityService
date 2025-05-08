using IdentityServiceApi.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityServiceApi.Extensions
{
    /// <summary>
    ///     Provides extension methods for configuring database services and health checks for the application.
    ///     This class contains methods to configure the application's database context and health check monitoring.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public static class DatabaseAndHealthCheckServiceExtensions
    {
        /// <summary>
        ///     Configures the database context and health checks for the application.
        ///     It sets up the application's main database connection and the health check monitoring for the database.
        ///     If in a development or staging environment, it also configures health checks UI and database storage.
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
        ///     The updated <see cref="IServiceCollection"/> with the database and health check services configured.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if a required connection string (either "ApplicationDatabase" or "HealthChecksDatabase") is missing
        ///     from the configuration.
        /// </exception>
        public static IServiceCollection AddDatabaseAndHealthChecks(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            var applicationDatabaseConnectionString = configuration.GetConnectionString("ApplicationDatabase")
                ?? throw new InvalidOperationException("ApplicationDatabase connection string is missing in the configuration.");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseLazyLoadingProxies()
                    .UseSqlServer(applicationDatabaseConnectionString);
            });

            if (environment.IsDevelopment() || environment.IsStaging())
            {
                var healthChecksDatabaseConnectionString = configuration.GetConnectionString("HealthChecksDatabase")
                    ?? throw new InvalidOperationException("HealthChecksDatabase connection string is missing in the configuration.");

                services.AddHealthChecks()
                    .AddDbContextCheck<ApplicationDbContext>("EntityFrameworkCore");

                services.AddHealthChecksUI()
                    .AddSqlServerStorage(healthChecksDatabaseConnectionString);

                services.AddDbContext<HealthChecksDbContext>(options =>
                {
                    options.UseSqlServer(healthChecksDatabaseConnectionString);
                });
            }

            return services;
        }
    }
}
