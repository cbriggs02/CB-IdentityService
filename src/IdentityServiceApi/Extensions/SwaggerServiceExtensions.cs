using Microsoft.OpenApi.Models;

namespace IdentityServiceApi.Extensions
{
    /// <summary>
    ///     Provides extension methods to configure Swagger services for the IdentityServiceApi.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    ///     @Updated: 2026
    /// </remarks>
    public static class SwaggerServiceExtensions
    {
        /// <summary>
        ///     Adds and configures Swagger generation and JWT bearer security to the service collection.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="IServiceCollection"/> to which the Swagger services will be added.
        /// </param>
        /// <returns>
        ///     The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.
        /// </returns>
        public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter: Bearer {your JWT token}"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {   
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {   
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            return services;
        }
    }
}
