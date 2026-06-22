using IdentityServiceApi.Data;
using IdentityServiceApi.Features.Authentication.Interfaces;
using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.Authentication.Services;
using IdentityServiceApi.Features.Authorization.Services;
using IdentityServiceApi.Features.UserManagement.Services;
using IdentityServiceApi.Shared.Utilities;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Features.UserManagement.Caching;
using IdentityServiceApi.Shared.Results;

namespace IdentityServiceApi.Extensions
{
    /// <summary>
    ///     Provides extension methods for registering application services to the dependency injection container.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    ///     @Updated: 2026
    /// </remarks>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Registers all application-layer services including authentication, authorization, logging, 
        ///     user management, and utilities.
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
            services.AddUserManagementServices();
            services.AddUtilityServices();
            services.AddServiceResultFactories();
            services.AddCacheServices();
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
            services.AddScoped<ILoggerService, LoggerService>();
            return services;
        }

        private static IServiceCollection AddServiceResultFactories(this IServiceCollection services)
        {
            services.AddScoped<IResultFactory, ResultFactory>();
            services.AddScoped<IUserResultFactory, UserResultFactory>();
            services.AddScoped<IUserLookupResultFactory, UserLookupResultFactory>();
            services.AddScoped<ILoginResultFactory, LoginResultFactory>();
            services.AddScoped<IRoleResultFactory, RoleResultFactory>();
            return services;
        }

        private static IServiceCollection AddCacheServices(this IServiceCollection services)
        {
            services.AddScoped<IUserCacheService, UserCacheService>();
            services.AddSingleton<IUserCacheKeyService, UserCacheKeyService>();
            return services;
        }

        private static IServiceCollection AddMiscellaneousServices(this IServiceCollection services)
        {
            services.AddTransient<DbInitializer>();
            return services;
        }
    }
}
