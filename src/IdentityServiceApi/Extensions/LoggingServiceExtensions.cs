using Serilog;

namespace IdentityServiceApi.Extensions
{
    /// <summary>
    ///     Provides extension methods for configuring and adding Serilog logging to the application's host builder.
    ///     This class contains methods that allow setting up Serilog to handle logging for both development and production environments.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public static class LoggingServiceExtensions
    {
        /// <summary>
        ///     Configures and adds Serilog logging to the host builder, setting up different logging outputs based on the environment.
        ///     In development, logs are written to the console, while in production, logs are written to a file with daily rolling.
        /// </summary>
        /// <param name="hostBuilder">
        ///     The <see cref="IHostBuilder"/> instance used to build and configure the host.
        /// </param>
        /// <returns>
        ///     The updated <see cref="IHostBuilder"/> instance with Serilog logging configured.
        /// </returns>
        public static IHostBuilder AddSerilogLogging(this IHostBuilder hostBuilder)
        {
            hostBuilder.UseSerilog((context, services, config) =>
            {
                var env = context.HostingEnvironment;

                config.Enrich.FromLogContext();

                if (env.IsDevelopment())
                {
                    config.WriteTo.Console();
                }
                else
                {
                    config.WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day);
                }
            });

            return hostBuilder;
        }
    }
}
