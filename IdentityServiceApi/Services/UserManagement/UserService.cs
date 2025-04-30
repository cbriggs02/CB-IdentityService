using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Authorization;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityServiceApi.Models.Shared;
using IdentityServiceApi.Models.ServiceResultModels.Shared;
using IdentityServiceApi.Models.ServiceResultModels.UserManagement;
using IdentityServiceApi.Models.RequestModels.UserManagement;

namespace IdentityServiceApi.Services.UserManagement
{
    /// <summary>
    ///     Service responsible for interacting with user-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserServiceResultFactory _userServiceResultFactory;
        private readonly IPasswordHistoryCleanupService _cleanupService;
        private readonly IPermissionService _permissionService;
        private readonly IParameterValidator _parameterValidator;
        private readonly IUserLookupService _userLookupService;
        private readonly ICountryService _countryService;
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userManager">
        ///     The user manager responsible for handling user management operations.
        /// </param>
        /// <param name="userServiceResultFactory">
        ///     The service used for creating the result objects being returned in operations.
        /// </param>
        /// <param name="cleanupService">
        ///     Service for cleaning up password history, such as removing old passwords after a user is deleted.
        /// </param>
        /// <param name="permissionService">
        ///     Service for validating and checking user permissions within the system.
        /// </param>
        /// <param name="parameterValidator">
        ///     The parameter validator service used for defense checking service parameters.
        /// </param>
        /// <param name="userLookupService">
        ///     The service used for looking up users in the system.
        /// </param>
        /// <param name="countryService">
        ///     The service used for managing country-specific data, such as country-related.
        /// </param>
        /// <param name="roleService">
        ///     The service responsible for managing user roles, including assigning, removing, and retrieving roles.
        /// </param>
        /// <param name="mapper">
        ///     Object mapper for converting between entities and data transfer objects (DTOs).
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when any of the provided service parameters are null.
        /// </exception>
        public UserService(UserManager<User> userManager, IUserServiceResultFactory userServiceResultFactory, IPasswordHistoryCleanupService cleanupService, IPermissionService permissionService, IParameterValidator parameterValidator, IUserLookupService userLookupService, ICountryService countryService, IRoleService roleService, IMapper mapper)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _userServiceResultFactory = userServiceResultFactory ?? throw new ArgumentNullException(nameof(userServiceResultFactory));
            _cleanupService = cleanupService ?? throw new ArgumentNullException(nameof(cleanupService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
            _userLookupService = userLookupService ?? throw new ArgumentNullException(nameof(userLookupService));
            _countryService = countryService ?? throw new ArgumentNullException(nameof(countryService));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        ///     Asynchronously retrieves a paginated list of users from the database based on the request parameters.
        /// </summary>
        /// <param name="request">
        ///     The request object containing pagination details such as the page number, page size
        ///     and account status for optional filtering.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="UserServiceListResult"/>
        ///     containing the list of users and associated pagination metadata.
        /// </returns>
        public async Task<UserServiceListResult> GetUsersAsync(UserListRequest request)
        {
            _parameterValidator.ValidateObjectNotNull(request, nameof(request));

            var query = _userManager.Users.AsQueryable();
            if (request.AccountStatus.HasValue)
            {
                query = query.Where(user => user.AccountStatus == request.AccountStatus.Value);
            }

            var totalCount = await query.CountAsync();
            var users = await query
                .Include(user => user.Country)
                .OrderBy(user => user.LastName)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .AsNoTracking()
                .ToListAsync();

            var userDTOs = users.Select(user => _mapper.Map<UserDTO>(user)).ToList();
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            PaginationModel paginationMetadata = new()
            {
                TotalCount = totalCount,
                PageSize = request.PageSize,
                CurrentPage = request.Page,
                TotalPages = totalPages
            };

            return new UserServiceListResult { Users = userDTOs, PaginationMetadata = paginationMetadata };
        }

        /// <summary>
        ///     Asynchronously retrieves a specific user from the database by their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to retrieve.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="UserServiceResult"/>
        ///     with the user's data if found.
        /// </returns>
        public async Task<UserServiceResult> GetUserAsync(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var permissionResult = await _permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return _userServiceResultFactory.UserOperationFailure(permissionResult.Errors.ToArray());
            }

            var userLookupResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return _userServiceResultFactory.UserOperationFailure(userLookupResult.Errors.ToArray());
            }

            var user = userLookupResult.UserFound;
            var userDTO = _mapper.Map<UserDTO>(user);

            return _userServiceResultFactory.UserOperationSuccess(userDTO);
        }

        /// <summary>
        ///     Asynchronously retrieves aggregated metrics for user states, including total, activated, and deactivated users.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains a <see cref="UserServiceStateMetricsResult"/>
        ///     with the user state metrics.
        /// </returns>
        public async Task<UserServiceStateMetricsResult> GetUserStateMetricsAsync()
        {
            var metrics = await _userManager.Users
                .AsNoTracking()
                .GroupBy(u => u.AccountStatus)
                .Select(g => new { AccountStatus = g.Key, Count = g.Count() })
                .ToListAsync();

            var activatedUsers = metrics.FirstOrDefault(m => m.AccountStatus == 1)?.Count ?? 0;
            var totalCount = metrics.Sum(m => m.Count);
            var deactivatedUsers = totalCount - activatedUsers;

            return new UserServiceStateMetricsResult { TotalCount = totalCount, ActivatedUsers = activatedUsers, DeactivatedUsers = deactivatedUsers };
        }

        /// <summary>
        ///     Asynchronously retrieves aggregated metrics representing the number of users created on each date.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains a <see cref="UserServiceCreationStatsResult"/>
        ///     with the user creation date metrics.
        /// </returns>
        public async Task<UserServiceCreationStatsResult> GetUserCreationStatsAsync()
        {
            var stats = await _userManager.Users
                .AsNoTracking()
                .GroupBy(u => u.CreatedAt.Date)
                .Select( g=> new UserCreationStatDTO { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return new UserServiceCreationStatsResult { UserCreationStats = stats };
        }

        /// <summary>
        ///     Asynchronously creates a new user in the system and stores their details in the database.
        /// </summary>
        /// <param name="user">
        ///     A <see cref="UserDTO"/> object containing the user's details.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="UserServiceResult"/>
        ///     indicating whether the user creation was successful.
        ///     - If successful, returns the details of the created user.
        ///     - If the user already exists or an error occurs, returns relevant error messages.
        /// </returns>
        public async Task<UserServiceResult> CreateUserAsync(UserDTO user)
        {
            ValidateUserDTO(user);

            var country = await _countryService.FindCountryByIdAsync(user.CountryId);
            if (country == null)
            {
                return _userServiceResultFactory.UserOperationFailure(new[] { ErrorMessages.User.CountryNotFound });
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
                return _userServiceResultFactory.UserOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }

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
        ///     A task that represents the asynchronous operation, returning a <see cref="ServiceResult"/>
        ///     indicating the success or failure of the update operation.
        /// </returns>
        public async Task<ServiceResult> UpdateUserAsync(string id, UserDTO user)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            ValidateUserDTO(user);

            var permissionResult = await _permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return _userServiceResultFactory.GeneralOperationFailure(permissionResult.Errors.ToArray());
            }

            var userLookupResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return _userServiceResultFactory.GeneralOperationFailure(userLookupResult.Errors.ToArray());
            }

            var country = await _countryService.FindCountryByIdAsync(user.CountryId);
            if (country == null)
            {
                return _userServiceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.User.CountryNotFound });
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
                return _userServiceResultFactory.GeneralOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }

            return _userServiceResultFactory.GeneralOperationSuccess();
        }

        /// <summary>
        ///     Asynchronously deletes a user from the system based on their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to delete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="ServiceResult"/>
        ///     indicating whether the deletion was successful.
        ///     - If successful, deletes associated password history as well.
        ///     - If the user is not found or an error occurs, returns relevant error messages.
        /// </returns>
        public async Task<ServiceResult> DeleteUserAsync(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var permissionResult = await _permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return _userServiceResultFactory.GeneralOperationFailure(permissionResult.Errors.ToArray());
            }

            var userLookupServiceResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupServiceResult.Success)
            {
                return _userServiceResultFactory.UserOperationFailure(userLookupServiceResult.Errors.ToArray());
            }

            var user = userLookupServiceResult.UserFound;

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return _userServiceResultFactory.GeneralOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }

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
        ///     A task that represents the asynchronous operation, returning a <see cref="ServiceResult"/>
        ///     indicating the success or failure of the account activation.
        /// </returns>
        public async Task<ServiceResult> ActivateUserAsync(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var permissionResult = await _permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return _userServiceResultFactory.GeneralOperationFailure(permissionResult.Errors.ToArray());
            }

            var userLookupServiceResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupServiceResult.Success)
            {
                return _userServiceResultFactory.UserOperationFailure(userLookupServiceResult.Errors.ToArray());
            }

            var user = userLookupServiceResult.UserFound;

            if (user.AccountStatus != 0)
            {
                return _userServiceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.User.AlreadyActivated });
            }

            user.AccountStatus = 1;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return _userServiceResultFactory.GeneralOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }

            return _userServiceResultFactory.GeneralOperationSuccess();
        }

        /// <summary>
        ///     Asynchronously deactivates a user account in the system based on their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to deactivate.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="ServiceResult"/>
        ///     indicating the success or failure of the account deactivation.
        /// </returns>
        public async Task<ServiceResult> DeactivateUserAsync(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var permissionResult = await _permissionService.ValidatePermissionsAsync(id);
            if (!permissionResult.Success)
            {
                return _userServiceResultFactory.GeneralOperationFailure(permissionResult.Errors.ToArray());
            }

            var userLookupServiceResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupServiceResult.Success)
            {
                return _userServiceResultFactory.UserOperationFailure(userLookupServiceResult.Errors.ToArray());
            }

            var user = userLookupServiceResult.UserFound;

            if (user.AccountStatus != 1)
            {
                return _userServiceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.User.NotActivated });
            }

            user.AccountStatus = 0;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return _userServiceResultFactory.GeneralOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }

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
        ///     A task representing the asynchronous operation, returning a <see cref="ServiceResult"/>.
        ///     The result indicates the assignment status:
        ///     - If successful, Success is true.
        ///     - If the user ID or role name is invalid, an error message is returned.
        ///     - If the user already has the role, an error is returned.
        ///     - If an error occurs during the assignment, an error message is returned.
        /// </returns>
        public async Task<ServiceResult> AssignRoleAsync(string id, string roleName)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            _parameterValidator.ValidateNotNullOrEmpty(roleName, nameof(roleName));

            return await _roleService.AssignRoleAsync(id, roleName);
        }

        /// <summary>
        ///     Asynchronously removes a specified role to a user identified by their unique ID.
        /// </summary>
        /// <param name="id">
        ///     The unique ID of the user to whom the role is being removed.
        /// </param>
        /// <param name="roleName">
        ///     The name of the role to remove from the user.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation, returning a <see cref="ServiceResult"/>
        ///     indicating the removal status:
        ///     - If successful, returns a result with Success set to true.
        ///     - If the user ID or role name is invalid, returns an error message.
        ///     - If an error occurs during removal, returns a result with an error message.
        /// </returns>
        public async Task<ServiceResult> RemoveRoleAsync(string id, string roleName)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            _parameterValidator.ValidateNotNullOrEmpty(roleName, nameof(roleName));

            return await _roleService.RemoveRoleAsync(id, roleName);
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
