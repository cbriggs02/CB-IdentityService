using AspNetCoreRateLimit;
using IdentityServiceApi.Data;
using IdentityServiceApi.Extensions;
using IdentityServiceApi.Mapping;
using IdentityServiceApi.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace IdentityServiceApi
{
    /// <summary>
    ///     Entry point class for the ASP.NET Core application,
    /// </summary>
    /// <remarks>
    ///     This application is designed to provide secure and scalable web services for managing 
    ///     user accounts, roles, and permissions. It includes features like authentication, role-based 
    ///     access control, and CRUD operations for user data.
    /// -----------------------------------------------------------------------------------------------
    ///     Key configurations in this file include:
    ///     - Middleware for request handling (e.g., exception handling, performance monitoring, token validating).
    ///     - ASP.NET Identity, JWT Bearer Authentication.
    ///     - Dependency injection setup for services.
    ///     - Integration of Swagger for API documentation.
    ///     - API Versioning and integration of an API Health Checks UI.
    ///     - Database initialization and migration.
    ///     - Cors policies, In Memory Caching setup, Rate limitation configuration. 
    /// -----------------------------------------------------------------------------------------------
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddSerilogLogging();
            builder.Services.AddDatabaseAndHealthChecks(builder.Configuration, builder.Environment);
            builder.Services.AddIdentityServices();
            builder.Services.AddMemoryCache();
            builder.Services.AddApiConfiguration();
            builder.Services.AddApplicationServices();
            builder.Services.AddSwaggerServices();
            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
            builder.Services.AddAuthenticationServices(builder.Configuration);
            builder.Services.AddCorsPolicy();
            builder.Services.AddRateLimiting();

            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
                await dbInitializer.InitializeDatabaseAsync(app);
            }

            app.UseDevelopmentAndStagingSetup();

            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseMiddleware<PerformanceMonitoringMiddleware>();

            if (app.Environment.IsProduction())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("AllowAdminApp");
            app.UseIpRateLimiting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<TokenValidatorMiddleware>();

            app.MapControllers();
            app.MapHealthChecks("/health");
            app.Run();
        }
    }
}
