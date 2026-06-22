using IdentityServiceApi.Features.Authorization.Caching;
using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Features.Authorization.Models;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Shared.Results;
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
        /// <summary>
        ///     Asynchronously retrieves all roles from the database, ordered by name.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation that returns an <see cref=" RoleListResult"/>
        ///     containing all roles in the system.
        /// </returns>
        public async Task<RoleListResult> GetRolesAsync()
        {
            if (cache.TryGetValue(RolesCacheKeys.RoleList, out RoleListResult? cachedRoles) && cachedRoles != null)
            {
                return cachedRoles;
            }
            var roles = await roleManager.Roles
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new RoleDTO { Id = x.Id, Name = x.Name ?? string.Empty })
                .ToListAsync();

            var result = new RoleListResult { Roles = roles };
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.NeverRemove);

            cache.Set(RolesCacheKeys.RoleList, cachedRoles, cacheOptions);
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
            parameterValidator.ValidateNotNullOrEmpty(roleId, nameof(roleId));

            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return serviceResultFactory.RoleOperationFailure([ErrorMessages.Role.NotFound], ErrorType.NotFound);
            }

            var roleDto = new RoleDTO
            {
                Id = role.Id,
                Name = role.Name?.Trim() ?? string.Empty
            };

            return serviceResultFactory.RoleOperationSuccess(roleDto);
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
            parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            parameterValidator.ValidateNotNullOrEmpty(roleName, nameof(roleName));

            var userLookupResult = await userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return serviceResultFactory.GeneralOperationFailure([.. userLookupResult.Errors], userLookupResult.ErrorType);
            }

            var user = userLookupResult.UserFound;
            if (user.AccountStatus != 1)
            {
                return serviceResultFactory.GeneralOperationFailure([ErrorMessages.Role.InactiveUser], ErrorType.InvalidState);
            }

            if (!await roleManager.RoleExistsAsync(roleName))
            {
                return serviceResultFactory.GeneralOperationFailure([ErrorMessages.Role.InvalidRole], ErrorType.Validation);
            }

            var existingRoles = await userManager.GetRolesAsync(user);
            if (existingRoles.Any())
            {
                return serviceResultFactory.GeneralOperationFailure([ErrorMessages.Role.UserAlreadyHasRole], ErrorType.InvalidState);
            }

            var result = await userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                return serviceResultFactory.GeneralOperationFailure([.. result.Errors.Select(e => e.Description)], ErrorType.Validation);
            }

            LogRoleOperation($"Assigned role '{roleName}' to user with ID {id}.");
            return serviceResultFactory.GeneralOperationSuccess();
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
            parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var userLookupResult = await userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return serviceResultFactory.GeneralOperationFailure([.. userLookupResult.Errors], userLookupResult.ErrorType);
            }

            var user = userLookupResult.UserFound;

            var roles = await userManager.GetRolesAsync(user);
            if (!roles.Any())
            {
                return serviceResultFactory.GeneralOperationFailure([ErrorMessages.Role.MissingRole], ErrorType.InvalidState);
            }

            var roleName = roles.First();

            var result = await userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                return serviceResultFactory.GeneralOperationFailure([.. result.Errors.Select(e => e.Description)], ErrorType.Validation);
            }

            LogRoleOperation($"Removed role '{roleName}' to user with ID {id}.");
            return serviceResultFactory.GeneralOperationSuccess();
        }

        private void LogRoleOperation(string message)
        {
            var logEntry = new LogEntry
            {
                LogLevel = LogLevel.Information,
                LogSource = LogSource.RoleService,
                Message = message
            };

            loggerService.LogData(logEntry);
        }
    }
}
