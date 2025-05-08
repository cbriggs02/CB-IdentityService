using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace IdentityServiceApi.Extensions
{
    /// <summary>
    ///     Provides extension methods for configuring development and staging-specific middleware in the ASP.NET Core application.
    ///     This includes setting up Swagger, health checks, and health check UI.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public static class ProgramExtensions
    {
        /// <summary>
        ///     Configures middleware for development and staging environments.
        ///     This method sets up Swagger UI for API documentation and configures health checks for the application and its database.
        /// </summary>
        /// <param name="app">
        ///     The <see cref="WebApplication"/> instance that the middleware will be applied to.
        /// </param>
        public static void UseDevelopmentAndStagingSetup(this WebApplication app)
        {
            if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentityServiceApi V1");
                    c.RoutePrefix = string.Empty;
                });

                app.UseHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                app.UseHealthChecks("/health/database", new HealthCheckOptions()
                {
                    Predicate = registration => registration.Name == "EntityFrameworkCore",
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                app.UseHealthChecksUI(config => config.UIPath = "/health-ui");
            }
        }
    }
}
