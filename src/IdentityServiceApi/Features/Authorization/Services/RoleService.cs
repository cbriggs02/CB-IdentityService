using IdentityServiceApi.Features.Authorization.Caching;
using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Features.Authorization.Models;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Shared.ResultFactories;
using IdentityServiceApi.Shared.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityServiceApi.Features.Authorization.Services
{
    /// <summary>
    ///     Service responsible for interacting with role-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class RoleService(IMemoryCache cache, RoleManager<IdentityRole> roleManager, UserManager<User> userManager, IParameterValidator parameterValidator, IRoleResultFactory serviceResultFactory, IUserLookupService userLookupService, ILoggerService loggerService) : IRoleService
    {
        private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        private readonly RoleManager<IdentityRole> _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        private readonly UserManager<User> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IParameterValidator _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
        private readonly IRoleResultFactory _serviceResultFactory = serviceResultFactory ?? throw new ArgumentNullException(nameof(serviceResultFactory));
        private readonly IUserLookupService _userLookupService = userLookupService ?? throw new ArgumentNullException(nameof(userLookupService));
        private readonly ILoggerService _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));

        /// <summary>
        ///     Asynchronously retrieves all roles from the database, ordered by name.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation that returns an <see cref=" RoleListResult"/>
        ///     containing all roles in the system.
        /// </returns>
        public async Task<RoleListResult> GetRolesAsync()
        {
            if (_cache.TryGetValue(RolesCacheKeys.RoleList, out RoleListResult? cachedRoles) && cachedRoles != null)
            {
                return cachedRoles;
            }
            var roles = await _roleManager.Roles
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new RoleDTO { Id = x.Id, Name = x.Name ?? string.Empty })
                .ToListAsync();

            var result = new RoleListResult { Roles = roles };
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.NeverRemove);

            _cache.Set(RolesCacheKeys.RoleList, cachedRoles, cacheOptions);
            return result;
        }

        /// <summary>
        ///     Asynchronously retrieves a role from the database using the specified role ID.
        /// </summary>
        /// <param name="roleId">
        ///     The unique identifier of the role to retrieve.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns a <see cref="RoleResult"/>
        ///     containing the role information if found, or an error result if the role does not exist.
        /// </returns>
        public async Task<RoleResult> GetRoleAsync(string roleId)
        {
            _parameterValidator.ValidateNotNullOrEmpty(roleId, nameof(roleId));

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return _serviceResultFactory.RoleOperationFailure([ErrorMessages.Role.NotFound]);
            }

            var roleDto = new RoleDTO
            {
                Id = role.Id,
                Name = role.Name?.Trim() ?? string.Empty
            };

            return _serviceResultFactory.RoleOperationSuccess(roleDto);
        }

        /// <summary>
        ///     Asynchronously assigns a specified role to a user identified by their unique ID.
        /// </summary>
        /// <param name="id">
        ///     The unique ID of the user to whom the role is being assigned.
        /// </param>
        /// <param name="roleName">
        ///     The name of the role to assign to the user.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation, returning a <see cref="Result"/>.
        ///     The result indicates the assignment status:
        ///     - If successful, Success is true.
        ///     - If the user ID or role name is invalid, an error message is returned.
        ///     - If the user already has a role, an error is returned.
        ///     - If an error occurs during the assignment, an error message is returned.
        /// </returns>
        public async Task<Result> AssignRoleAsync(string id, string roleName)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            _parameterValidator.ValidateNotNullOrEmpty(roleName, nameof(roleName));

            var userLookupResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return _serviceResultFactory.GeneralOperationFailure([.. userLookupResult.Errors]);
            }

            var user = userLookupResult.UserFound;
            if (user.AccountStatus != 1)
            {
                return _serviceResultFactory.GeneralOperationFailure([ErrorMessages.Role.InactiveUser]);
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return _serviceResultFactory.GeneralOperationFailure([ErrorMessages.Role.InvalidRole]);
            }

            var existingRoles = await _userManager.GetRolesAsync(user);
            if (existingRoles.Any())
            {
                return _serviceResultFactory.GeneralOperationFailure([ErrorMessages.Role.UserAlreadyHasRole]);
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                return _serviceResultFactory.GeneralOperationFailure([.. result.Errors.Select(e => e.Description)]);
            }

            LogRoleOperation($"Assigned role '{roleName}' to user with ID {id}.");
            return _serviceResultFactory.GeneralOperationSuccess();
        }

        /// <summary>
        ///     Asynchronously removes an assigned role from a user identified by their unique ID.
        /// </summary>
        /// <param name="id">
        ///     The unique ID of the user to whom the role is being removed.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation, returning a <see cref="Result"/>
        ///     indicating the removal status:
        ///     - If successful, returns a result with Success set to true.
        ///     - If the user ID is invalid, returns an error message.
        ///     - If an error occurs during removal, returns a result with an error message.
        /// </returns>
        public async Task<Result> RemoveRoleAsync(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var userLookupResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return _serviceResultFactory.GeneralOperationFailure([.. userLookupResult.Errors]);
            }

            var user = userLookupResult.UserFound;

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any())
            {
                return _serviceResultFactory.GeneralOperationFailure([ErrorMessages.Role.MissingRole]);
            }

            var roleName = roles.First();

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                return _serviceResultFactory.GeneralOperationFailure([.. result.Errors.Select(e => e.Description)]);
            }

            LogRoleOperation($"Removed role '{roleName}' to user with ID {id}.");
            return _serviceResultFactory.GeneralOperationSuccess();
        }

        private void LogRoleOperation(string message)
        {
            var logEntry = new LogEntry
            {
                LogLevel = LogLevel.Information,
                LogSource = LogSource.RoleService,
                Message = message
            };

            _loggerService.LogData(logEntry);
        }
    }
}
