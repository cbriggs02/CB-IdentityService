namespace IdentityServiceApi.Extensions
{
    /// <summary>
    ///     Provides extension methods for configuring CORS (Cross-Origin Resource Sharing) policies.
    ///     This class contains methods to add CORS policies to the service container.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public static class CorsServiceExtensions
    {
        /// <summary>
        ///     Adds a CORS policy to the service collection to allow cross-origin requests from the specified origins.
        ///     This policy specifically allows requests from "http://localhost:4200" and enables the use of any HTTP method and headers.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="IServiceCollection"/> to which the CORS policy will be added.
        /// </param>
        /// <returns>
        ///     The updated <see cref="IServiceCollection"/> with the CORS policy configured.
        /// </returns>
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAdminApp",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:4200")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });

            return services;
        }
    }
}
