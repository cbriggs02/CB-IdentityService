using AutoMapper;
using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.DTOs;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Requests;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Shared.Models;
using IdentityServiceApi.Shared.Results;
using IdentityServiceApi.Shared.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityServiceApi.Features.UserManagement.Services
{
    /// <summary>
    ///     Service responsible for interacting with user-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class UserService(IMemoryCache cache, IUserCacheKeyService cacheKeyService, IUserCacheService cacheService, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IUserResultFactory userServiceResultFactory, IPasswordHistoryCleanupService cleanupService, IPermissionService permissionService, IParameterValidator parameterValidator, IUserLookupService userLookupService, ICountryService countryService, IRoleService roleService, IMapper mapper, ILoggerService loggerService) : IUserService
    {
        /// <summary>
        ///     Asynchronously retrieves a paginated list of users from the database based on the request parameters.
        /// </summary>
        /// <param name="request">
        ///     The request object containing pagination details such as the page number, page size
        ///     and account status for optional filtering.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="UserListResult"/>
        ///     containing the list of users and associated pagination metadata.
        /// </returns>
        public async Task<UserListResult> GetUsersAsync(UserListRequest request)
        {
            parameterValidator.ValidateObjectNotNull(request, nameof(request));

            var cacheKey = cacheKeyService.GetUserListKey(request.Page, request.PageSize, request.AccountStatus);
            if (cache.TryGetValue(cacheKey, out UserListResult? cachedUsers) && cachedUsers != null)
            {
                return cachedUsers;
            }

            var query = userManager.Users.AsQueryable();
            if (request.AccountStatus.HasValue)
            {
                query = query.Where(user => user.AccountStatus == request.AccountStatus.Value);
            }

            var totalCount = await query.CountAsync();
            var users = await query
                .Select(x => new SimplifiedUserDTO
                {
                    Id = x.Id,
                    UserName = x.UserName ?? string.Empty,
                    Name = $"{x.FirstName} {x.LastName}",
                    AccountStatus = x.AccountStatus
                })
                .OrderBy(user => user.UserName)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .AsNoTracking()
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            PaginationModel paginationMetadata = new()
            {
                TotalCount = totalCount,
                PageSize = request.PageSize,
                CurrentPage = request.Page,
                TotalPages = totalPages
            };

            var result = new UserListResult { Users = users, PaginationMetadata = paginationMetadata };
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(3))
                .SetSlidingExpiration(TimeSpan.FromMinutes(1))
                .SetPriority(CacheItemPriority.Normal);

            cache.Set(cacheKey, result, cacheOptions);
            return result;
        }

        /// <summary>
        ///     Asynchronously retrieves a specific user from the database by their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to retrieve.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="UserResult"/>
        ///     with the user's data if found.
        /// </returns>
        public async Task<UserResult> GetUserAsync(string id)
        {
            parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var permissionResult = await permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return userServiceResultFactory.UserOperationFailure([.. permissionResult.Errors], permissionResult.ErrorType);
            }

            var userLookupResult = await userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return userServiceResultFactory.UserOperationFailure([.. userLookupResult.Errors], userLookupResult.ErrorType);
            }

            var user = userLookupResult.UserFound;
            var roleId = await GetUserRoleIdAsync(user);
            var userDTO = mapper.Map<UserDTO>(user);
            userDTO.RoleId = roleId;

            return userServiceResultFactory.UserOperationSuccess(userDTO);
        }

        /// <summary>
        ///     Asynchronously creates a new user in the system and stores their details in the database.
        /// </summary>
        /// <param name="user">
        ///     A <see cref="UserDTO"/> object containing the user's details.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="UserResult"/>
        ///     indicating whether the user creation was successful.
        ///     - If successful, returns the details of the created user.
        ///     - If the user already exists or an error occurs, returns relevant error messages.
        /// </returns>
        public async Task<UserResult> CreateUserAsync(UserDTO user)
        {
            ValidateUserDTO(user);

            var country = await countryService.FindCountryByIdAsync(user.CountryId);
            if (country == null)
            {
                return userServiceResultFactory.UserOperationFailure([ErrorMessages.User.CountryNotFound], ErrorType.UnprocessableEntity);
            }

            var newUser = new User
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CountryId = user.CountryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            var result = await userManager.CreateAsync(newUser);
            if (!result.Succeeded)
            {
                return userServiceResultFactory.UserOperationFailure([.. result.Errors.Select(e => e.Description)], ErrorType.Validation);
            }

            cacheService.ClearUserListCache();

            var returnUser = new UserDTO
            {
                Id = newUser.Id,
                UserName = newUser.UserName,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Email = newUser.Email,
                PhoneNumber = newUser.PhoneNumber,
                CountryId = newUser.CountryId,
                CountryName = country.Name
            };

            return userServiceResultFactory.UserOperationSuccess(returnUser);
        }

        /// <summary>
        ///     Asynchronously updates an existing user's details in the system based on their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to update.
        /// </param>
        /// <param name="user">
        ///     A <see cref="UserDTO"/> object containing the updated user details.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="Result"/>
        ///     indicating the success or failure of the update operation.
        /// </returns>
        public async Task<Result> UpdateUserAsync(string id, UserDTO user)
        {
            parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            ValidateUserDTO(user);

            var permissionResult = await permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return userServiceResultFactory.GeneralOperationFailure([.. permissionResult.Errors], permissionResult.ErrorType);
            }

            var userLookupResult = await userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return userServiceResultFactory.GeneralOperationFailure([.. userLookupResult.Errors], userLookupResult.ErrorType);
            }

            var country = await countryService.FindCountryByIdAsync(user.CountryId);
            if (country == null)
            {
                return userServiceResultFactory.GeneralOperationFailure([ErrorMessages.User.CountryNotFound], ErrorType.UnprocessableEntity);
            }

            var existingUser = userLookupResult.UserFound;
            existingUser.UserName = user.UserName;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.CountryId = user.CountryId;
            existingUser.UpdatedAt = DateTime.UtcNow;

            var result = await userManager.UpdateAsync(existingUser);
            if (!result.Succeeded)
            {
                return userServiceResultFactory.GeneralOperationFailure([.. result.Errors.Select(e => e.Description)], ErrorType.Validation);
            }

            cacheService.ClearUserListCache();
            return userServiceResultFactory.GeneralOperationSuccess();
        }

        /// <summary>
        ///     Asynchronously deletes a user from the system based on their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to delete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="Result"/>
        ///     indicating whether the deletion was successful.
        ///     - If successful, deletes associated password history as well.
        ///     - If the user is not found or an error occurs, returns relevant error messages.
        /// </returns>
        public async Task<Result> DeleteUserAsync(string id)
        {
            parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var permissionResult = await permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return userServiceResultFactory.GeneralOperationFailure([.. permissionResult.Errors], permissionResult.ErrorType);
            }

            var userLookupServiceResult = await userLookupService.FindUserByIdAsync(id);
            if (!userLookupServiceResult.Success)
            {
                return userServiceResultFactory.UserOperationFailure([.. userLookupServiceResult.Errors], userLookupServiceResult.ErrorType);
            }

            var user = userLookupServiceResult.UserFound;

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return userServiceResultFactory.GeneralOperationFailure([.. result.Errors.Select(e => e.Description)], ErrorType.Validation);
            }

            LogUserOperation($"User with ID {id} has been deleted.");
            cacheService.ClearUserListCache();

            // delete all stored passwords for user once user is deleted for data clean up.
            await cleanupService.DeletePasswordHistoryAsync(id);
            return userServiceResultFactory.GeneralOperationSuccess();
        }

        /// <summary>
        ///     Asynchronously activates a user account in the system based on their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to activate.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="Result"/>
        ///     indicating the success or failure of the account activation.
        /// </returns>
        public async Task<Result> ActivateUserAsync(string id)
        {
            parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var permissionResult = await permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return userServiceResultFactory.GeneralOperationFailure([.. permissionResult.Errors], permissionResult.ErrorType);
            }

            var userLookupServiceResult = await userLookupService.FindUserByIdAsync(id);
            if (!userLookupServiceResult.Success)
            {
                return userServiceResultFactory.UserOperationFailure([.. userLookupServiceResult.Errors], userLookupServiceResult.ErrorType);
            }

            var user = userLookupServiceResult.UserFound;

            if (user.AccountStatus != 0)
            {
                return userServiceResultFactory.GeneralOperationFailure([ErrorMessages.User.AlreadyActivated], ErrorType.InvalidState);
            }

            user.AccountStatus = 1;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return userServiceResultFactory.GeneralOperationFailure([.. result.Errors.Select(e => e.Description)], ErrorType.Validation);
            }

            LogUserOperation($"User with ID {id} has been activated.");
            cacheService.ClearUserListCache();
            return userServiceResultFactory.GeneralOperationSuccess();
        }

        /// <summary>
        ///     Asynchronously deactivates a user account in the system based on their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to deactivate.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="Result"/>
        ///     indicating the success or failure of the account deactivation.
        /// </returns>
        public async Task<Result> DeactivateUserAsync(string id)
        {
            parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var permissionResult = await permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return userServiceResultFactory.GeneralOperationFailure([.. permissionResult.Errors], permissionResult.ErrorType);
            }

            var userLookupServiceResult = await userLookupService.FindUserByIdAsync(id);
            if (!userLookupServiceResult.Success)
            {
                return userServiceResultFactory.UserOperationFailure([.. userLookupServiceResult.Errors], userLookupServiceResult.ErrorType);
            }

            var user = userLookupServiceResult.UserFound;

            if (user.AccountStatus != 1)
            {
                return userServiceResultFactory.GeneralOperationFailure([ErrorMessages.User.NotActivated], ErrorType.InvalidState);
            }

            user.AccountStatus = 0;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return userServiceResultFactory.GeneralOperationFailure([.. result.Errors.Select(e => e.Description)], ErrorType.Validation);
            }

            LogUserOperation($"User with ID {id} has been deactivated.");
            cacheService.ClearUserListCache();
            return userServiceResultFactory.GeneralOperationSuccess();
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
        ///     - If the user already has the role, an error is returned.
        ///     - If an error occurs during the assignment, an error message is returned.
        /// </returns>
        public async Task<Result> AssignRoleAsync(string id, string roleName)
        {
            parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            parameterValidator.ValidateNotNullOrEmpty(roleName, nameof(roleName));
            return await roleService.AssignRoleAsync(id, roleName);
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
            return await roleService.RemoveRoleAsync(id);
        }

        private async Task<string?> GetUserRoleIdAsync(User user)
        {
            parameterValidator.ValidateObjectNotNull(user, nameof(user));

            var roleNames = await userManager.GetRolesAsync(user);
            if (!roleNames.Any())
            {
                return null;
            }

            var roleName = roleNames.First();
            var role = await roleManager.FindByNameAsync(roleName);
            return role?.Id;
        }

        private void LogUserOperation(string message)
        {
            var logEntry = new LogEntry
            {
                LogLevel = LogLevel.Information,
                LogSource = LogSource.UserService,
                Message = message
            };

            loggerService.LogData(logEntry);
        }

        private void ValidateUserDTO(UserDTO user)
        {
            parameterValidator.ValidateObjectNotNull(user, nameof(user));
            parameterValidator.ValidateNotNullOrEmpty(user.UserName, nameof(user.UserName));
            parameterValidator.ValidateNotNullOrEmpty(user.FirstName, nameof(user.FirstName));
            parameterValidator.ValidateNotNullOrEmpty(user.LastName, nameof(user.LastName));
            parameterValidator.ValidateNotNullOrEmpty(user.Email, nameof(user.Email));
            parameterValidator.ValidateNotNullOrEmpty(user.PhoneNumber, nameof(user.PhoneNumber));
        }
    }
}
