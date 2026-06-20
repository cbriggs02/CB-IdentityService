using AutoMapper;
using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models;
using IdentityServiceApi.Features.UserManagement.Models.DTOs;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Requests;
using IdentityServiceApi.Features.UserManagement.Models.Results;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Shared.Models;
using IdentityServiceApi.Shared.ResultFactories;
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
        private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        private readonly IUserCacheKeyService _cacheKeyService = cacheKeyService ?? throw new ArgumentNullException(nameof(cacheKeyService));
        private readonly IUserCacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        private readonly UserManager<User> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly RoleManager<IdentityRole> _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        private readonly IUserResultFactory _userServiceResultFactory = userServiceResultFactory ?? throw new ArgumentNullException(nameof(userServiceResultFactory));
        private readonly IPasswordHistoryCleanupService _cleanupService = cleanupService ?? throw new ArgumentNullException(nameof(cleanupService));
        private readonly IPermissionService _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        private readonly IParameterValidator _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
        private readonly IUserLookupService _userLookupService = userLookupService ?? throw new ArgumentNullException(nameof(userLookupService));
        private readonly ICountryService _countryService = countryService ?? throw new ArgumentNullException(nameof(countryService));
        private readonly IRoleService _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILoggerService _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));

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
            _parameterValidator.ValidateObjectNotNull(request, nameof(request));

            var cacheKey = _cacheKeyService.GetUserListKey(request.Page, request.PageSize, request.AccountStatus);
            if (_cache.TryGetValue(cacheKey, out UserListResult? cachedUsers) && cachedUsers != null)
            {
                return cachedUsers;
            }

            var query = _userManager.Users.AsQueryable();
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

            _cache.Set(cacheKey, result, cacheOptions);
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
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var permissionResult = await _permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return _userServiceResultFactory.UserOperationFailure([.. permissionResult.Errors]);
            }

            var userLookupResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return _userServiceResultFactory.UserOperationFailure([.. userLookupResult.Errors]);
            }

            var user = userLookupResult.UserFound;
            var roleId = await GetUserRoleIdAsync(user);
            var userDTO = _mapper.Map<UserDTO>(user);
            userDTO.RoleId = roleId;

            return _userServiceResultFactory.UserOperationSuccess(userDTO);
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

            var country = await _countryService.FindCountryByIdAsync(user.CountryId);
            if (country == null)
            {
                return _userServiceResultFactory.UserOperationFailure([ErrorMessages.User.CountryNotFound]);
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

            var result = await _userManager.CreateAsync(newUser);
            if (!result.Succeeded)
            {
                return _userServiceResultFactory.UserOperationFailure([.. result.Errors.Select(e => e.Description)]);
            }

            _cacheService.ClearUserListCache();

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

            return _userServiceResultFactory.UserOperationSuccess(returnUser);
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
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            ValidateUserDTO(user);

            var permissionResult = await _permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return _userServiceResultFactory.GeneralOperationFailure([.. permissionResult.Errors]);
            }

            var userLookupResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return _userServiceResultFactory.GeneralOperationFailure([.. userLookupResult.Errors]);
            }

            var country = await _countryService.FindCountryByIdAsync(user.CountryId);
            if (country == null)
            {
                return _userServiceResultFactory.GeneralOperationFailure([ErrorMessages.User.CountryNotFound]);
            }

            var existingUser = userLookupResult.UserFound;
            existingUser.UserName = user.UserName;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.CountryId = user.CountryId;
            existingUser.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(existingUser);
            if (!result.Succeeded)
            {
                return _userServiceResultFactory.GeneralOperationFailure([.. result.Errors.Select(e => e.Description)]);
            }

            _cacheService.ClearUserListCache();
            return _userServiceResultFactory.GeneralOperationSuccess();
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
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var permissionResult = await _permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return _userServiceResultFactory.GeneralOperationFailure([.. permissionResult.Errors]);
            }

            var userLookupServiceResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupServiceResult.Success)
            {
                return _userServiceResultFactory.UserOperationFailure([.. userLookupServiceResult.Errors]);
            }

            var user = userLookupServiceResult.UserFound;

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return _userServiceResultFactory.GeneralOperationFailure([.. result.Errors.Select(e => e.Description)]);
            }

            LogUserOperation($"User with ID {id} has been deleted.");
            _cacheService.ClearUserListCache();

            // delete all stored passwords for user once user is deleted for data clean up.
            await _cleanupService.DeletePasswordHistoryAsync(id);
            return _userServiceResultFactory.GeneralOperationSuccess();
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
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var permissionResult = await _permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return _userServiceResultFactory.GeneralOperationFailure([.. permissionResult.Errors]);
            }

            var userLookupServiceResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupServiceResult.Success)
            {
                return _userServiceResultFactory.UserOperationFailure([.. userLookupServiceResult.Errors]);
            }

            var user = userLookupServiceResult.UserFound;

            if (user.AccountStatus != 0)
            {
                return _userServiceResultFactory.GeneralOperationFailure([ErrorMessages.User.AlreadyActivated]);
            }

            user.AccountStatus = 1;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return _userServiceResultFactory.GeneralOperationFailure([.. result.Errors.Select(e => e.Description)]);
            }

            LogUserOperation($"User with ID {id} has been activated.");
            _cacheService.ClearUserListCache();
            return _userServiceResultFactory.GeneralOperationSuccess();
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
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var permissionResult = await _permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return _userServiceResultFactory.GeneralOperationFailure([.. permissionResult.Errors]);
            }

            var userLookupServiceResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupServiceResult.Success)
            {
                return _userServiceResultFactory.UserOperationFailure([.. userLookupServiceResult.Errors]);
            }

            var user = userLookupServiceResult.UserFound;

            if (user.AccountStatus != 1)
            {
                return _userServiceResultFactory.GeneralOperationFailure([ErrorMessages.User.NotActivated]);
            }

            user.AccountStatus = 0;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return _userServiceResultFactory.GeneralOperationFailure([.. result.Errors.Select(e => e.Description)]);
            }

            LogUserOperation($"User with ID {id} has been deactivated.");
            _cacheService.ClearUserListCache();
            return _userServiceResultFactory.GeneralOperationSuccess();
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
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            _parameterValidator.ValidateNotNullOrEmpty(roleName, nameof(roleName));
            return await _roleService.AssignRoleAsync(id, roleName);
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
            return await _roleService.RemoveRoleAsync(id);
        }

        private async Task<string?> GetUserRoleIdAsync(User user)
        {
            _parameterValidator.ValidateObjectNotNull(user, nameof(user));

            var roleNames = await _userManager.GetRolesAsync(user);
            if (!roleNames.Any())
            {
                return null;
            }

            var roleName = roleNames.First();
            var role = await _roleManager.FindByNameAsync(roleName);
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

            _loggerService.LogData(logEntry);
        }

        private void ValidateUserDTO(UserDTO user)
        {
            _parameterValidator.ValidateObjectNotNull(user, nameof(user));
            _parameterValidator.ValidateNotNullOrEmpty(user.UserName, nameof(user.UserName));
            _parameterValidator.ValidateNotNullOrEmpty(user.FirstName, nameof(user.FirstName));
            _parameterValidator.ValidateNotNullOrEmpty(user.LastName, nameof(user.LastName));
            _parameterValidator.ValidateNotNullOrEmpty(user.Email, nameof(user.Email));
            _parameterValidator.ValidateNotNullOrEmpty(user.PhoneNumber, nameof(user.PhoneNumber));
        }
    }
}
