using AspNetCoreRateLimit;

namespace IdentityServiceApi.Extensions
{
    /// <summary>
    ///     Provides extension methods to configure rate limiting services.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public static class RateLimitingExtensions
    {
        /// <summary>
        ///     Adds IP rate limiting configuration to the service collection.
        ///     Configures rules to limit requests per IP address for specified endpoints.
        /// </summary>
        /// <param name="services">
        ///     The IServiceCollection to add the rate limiting configuration to.
        /// </param>
        /// <returns>
        ///     The IServiceCollection with IP rate limiting configured.
        /// </returns>
        public static IServiceCollection AddRateLimiting(this IServiceCollection services)
        {
            services.Configure<IpRateLimitOptions>(options =>
            {
                options.EnableEndpointRateLimiting = true;
                options.HttpStatusCode = 429;
                options.GeneralRules = new List<RateLimitRule>
                {
                    new()
                    {
                        Endpoint = "POST:/api/*/login/tokens",
                        Period = "1m",
                        Limit = 5
                    },
                    new()
                    {
                        Endpoint = "PATCH:/api/*/password/users/*/password",
                        Period = "1m",
                        Limit = 5
                    },
                    new()
                    {
                        Endpoint = "PUT:/api/*/password/users/*/password",
                        Period = "1m",
                        Limit = 5
                    },
                    new()
                    {
                        Endpoint = "POST:/api/*/users",
                        Period = "1m",
                        Limit = 10
                    },
                    new()
                    {
                        Endpoint = "PUT:/api/*/users/*",
                        Period = "1m",
                        Limit = 10
                    },
                    new()
                    {
                        Endpoint = "*",
                        Period = "1m",
                        Limit = 50
                    }
                };
            });

            services.AddInMemoryRateLimiting();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            return services;
        }
    }
}
