using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServiceApi.Extensions
{
    /// <summary>
    ///     Provides extension methods for configuring API-related services such as controllers, versioning, and API versioning conventions.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public static class ApiConfigurationExtensions
    {
        /// <summary>
        ///     Configures the API services including controllers, API versioning, and version-related options.
        ///     This method sets up default API versioning, filters for content type handling (JSON), and 
        ///     configures how versions are handled in URLs.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="IServiceCollection"/> used to register services for dependency injection.
        /// </param>
        /// <returns>
        ///     The updated <see cref="IServiceCollection"/> with API configuration services added.
        /// </returns>
        public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add(new ProducesAttribute("application/json"));
                options.Filters.Add(new ConsumesAttribute("application/json"));
            });

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }
    }
}
