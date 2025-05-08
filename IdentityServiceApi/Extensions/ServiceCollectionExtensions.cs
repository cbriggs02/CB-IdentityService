using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.Authentication;
using IdentityServiceApi.Interfaces.Authorization;
using IdentityServiceApi.Interfaces.Logging;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Services.Authentication;
using IdentityServiceApi.Services.Authorization;
using IdentityServiceApi.Services.Logging.Common;
using IdentityServiceApi.Services.Logging.Implementations;
using IdentityServiceApi.Services.Logging;
using IdentityServiceApi.Services.UserManagement;
using IdentityServiceApi.Services.Utilities.ResultFactories.Authentication;
using IdentityServiceApi.Services.Utilities.ResultFactories.Common;
using IdentityServiceApi.Services.Utilities.ResultFactories.Logging;
using IdentityServiceApi.Services.Utilities.ResultFactories.UserManagement;
using IdentityServiceApi.Services.Utilities;

namespace IdentityServiceApi.Extensions
{
    /// <summary>
    ///     Provides extension methods for registering application services to the dependency injection container.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Registers all application-layer services including authentication, authorization, logging, user management, and utilities.
        /// </summary>
        /// <param name="services">
        ///     The service collection to add services to.
        /// </param>
        /// <returns>
        ///     The updated <see cref="IServiceCollection"/> with application services registered.
        /// </returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddAuthenticationServices();
            services.AddAuthorizationServices();
            services.AddLoggingServices();
            services.AddUserManagementServices();
            services.AddUtilityServices();
            services.AddServiceResultFactories();
            services.AddMiscellaneousServices();
            return services;
        }

        private static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
        {
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IUserContextService, UserContextService>();
            return services;
        }

        private static IServiceCollection AddAuthorizationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IRoleService, RoleService>();
            return services;
        }

        private static IServiceCollection AddLoggingServices(this IServiceCollection services)
        {
            services.AddScoped<IAuditLoggerService, AuditLoggerService>();
            services.AddScoped<ILoggerService, LoggerService>();
            services.AddScoped<IAuthorizationLoggerService, AuthorizationLoggerService>();
            services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();
            services.AddScoped<IPerformanceLoggerService, PerformanceLoggerService>();
            services.AddScoped<ILoggingValidator, LoggingValidator>();
            return services;
        }

        private static IServiceCollection AddUserManagementServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserLookupService, UserLookupService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IPasswordHistoryService, PasswordHistoryService>();
            services.AddScoped<IPasswordHistoryCleanupService, PasswordHistoryCleanupService>();
            services.AddScoped<ICountryService, CountryService>();
            return services;
        }

        private static IServiceCollection AddUtilityServices(this IServiceCollection services)
        {
            services.AddScoped<IParameterValidator, ParameterValidator>();
            return services;
        }

        private static IServiceCollection AddServiceResultFactories(this IServiceCollection services)
        {
            services.AddScoped<IServiceResultFactory, ServiceResultFactory>();
            services.AddScoped<IUserServiceResultFactory, UserServiceResultFactory>();
            services.AddScoped<IUserLookupServiceResultFactory, UserLookupServiceResultFactory>();
            services.AddScoped<ILoginServiceResultFactory, LoginServiceResultFactory>();
            services.AddScoped<IAuditLoggerServiceResultFactory, AuditLoggerServiceResultFactory>();
            return services;
        }

        private static IServiceCollection AddMiscellaneousServices(this IServiceCollection services)
        {
            services.AddTransient<DbInitializer>();
            return services;
        }
    }
}
