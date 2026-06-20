using AspNetCoreRateLimit;
using IdentityServiceApi.Data;
using IdentityServiceApi.Extensions;
using IdentityServiceApi.Middleware;
using IdentityServiceApi.Shared.Mapping;

namespace IdentityServiceApi
{
    /// <summary>
    ///     Entry point class for the ASP.NET Core application built in .NET 10.0. 
    ///     This class is responsible for configuring and starting the web application, 
    ///     including setting up services, middleware, and request handling pipeline.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddSerilogLogging();
            builder.Services.AddApplicationDbContext(builder.Configuration, builder.Environment);
            builder.Services.AddIdentityServices();
            builder.Services.AddMemoryCache();
            builder.Services.AddApiConfiguration();
            builder.Services.AddApplicationServices();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerServices();
            builder.Services.AddAutoMapper(x => x.AddMaps(typeof(AutoMapperProfile)));
            builder.Services.AddAuthenticationServices(builder.Configuration);
            builder.Services.AddRateLimiting();

            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
                await dbInitializer.InitializeDatabaseAsync(app);
            }

            if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentityServiceApi V1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseMiddleware<GlobalExceptionMiddleware>();

            if (app.Environment.IsProduction())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseIpRateLimiting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}
