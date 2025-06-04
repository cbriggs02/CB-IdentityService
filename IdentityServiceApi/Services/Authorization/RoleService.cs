using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Authorization;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.ServiceResultModels.Authorization;
using IdentityServiceApi.Models.ServiceResultModels.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityServiceApi.Services.Authorization
{
    /// <summary>
    ///     Service responsible for interacting with role-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IParameterValidator _parameterValidator;
        private readonly IRoleServiceResultFactory _serviceResultFactory;
        private readonly IUserLookupService _userLookupService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RoleService"/> class.
        /// </summary>
        /// <param name="roleManager">
        ///     The role manager for handling role operations within the system.
        /// </param>
        /// <param name="userManager">
        ///     The user manager for handling user operations within the system.
        /// </param>
        /// <param name="parameterValidator">
        ///     The parameter validator service used for defense checking service parameters.
        /// </param>
        /// <param name="serviceResultFactory">
        ///     The service used for creating the result objects being returned in operations specific to roles.
        /// </param>
        /// <param name="userLookupService">'
        ///     The service used for looking up users in the system.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when any of the parameters are null.
        /// </exception>
        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<User> userManager, IParameterValidator parameterValidator, IRoleServiceResultFactory serviceResultFactory, IUserLookupService userLookupService)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
            _serviceResultFactory = serviceResultFactory ?? throw new ArgumentNullException(nameof(serviceResultFactory));
            _userLookupService = userLookupService ?? throw new ArgumentNullException(nameof(userLookupService));
        }

        /// <summary>
        ///     Asynchronously retrieves all roles from the database, ordered by name.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation that returns an <see cref=" RoleServiceListResult"/>
        ///     containing all roles in the system.
        /// </returns>
        public async Task<RoleServiceListResult> GetRolesAsync()
        {
            var roles = await _roleManager.Roles
                .OrderBy(x => x.Name)
                .Select(x => new RoleDTO { Id = x.Id, Name = x.Name })
                .AsNoTracking()
                .ToListAsync();

            return new RoleServiceListResult { Roles = roles };
        }

        /// <summary>
        ///     Asynchronously retrieves a role from the database using the specified role ID.
        /// </summary>
        /// <param name="roleId">
        ///     The unique identifier of the role to retrieve.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns a <see cref="RoleServiceResult"/>
        ///     containing the role information if found, or an error result if the role does not exist.
        /// </returns>
        public async Task<RoleServiceResult> GetRoleAsync(string roleId)
        {
            _parameterValidator.ValidateNotNullOrEmpty(roleId, nameof(roleId));

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return _serviceResultFactory.RoleOperationFailure(new[] { ErrorMessages.Role.NotFound });
            }

            return _serviceResultFactory.RoleOperationSuccess(new RoleDTO { Id = role.Id, Name = role.Name });
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

            var userLookupResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return _serviceResultFactory.GeneralOperationFailure(userLookupResult.Errors.ToArray());
            }

            var user = userLookupResult.UserFound;

            if (!IsUserActive(user))
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Role.InactiveUser });
            }

            if (!await DoesRoleExist(roleName))
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Role.InvalidRole });
            }

            var existingRoles = await _userManager.GetRolesAsync(user);
            if (existingRoles.Any())
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Role.UserAlreadyHasRole });
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                return _serviceResultFactory.GeneralOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }

            return _serviceResultFactory.GeneralOperationSuccess();
        }

        /// <summary>
        ///     Asynchronously removes an assigned role from a user identified by their unique ID.
        /// </summary>
        /// <param name="id">
        ///     The unique ID of the user to whom the role is being removed.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation, returning a <see cref="ServiceResult"/>
        ///     indicating the removal status:
        ///     - If successful, returns a result with Success set to true.
        ///     - If the user ID is invalid, returns an error message.
        ///     - If an error occurs during removal, returns a result with an error message.
        /// </returns>
        public async Task<ServiceResult> RemoveAssignedRoleAsync(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var userLookupResult = await _userLookupService.FindUserByIdAsync(id);
            if (!userLookupResult.Success)
            {
                return _serviceResultFactory.GeneralOperationFailure(userLookupResult.Errors.ToArray());
            }

            var user = userLookupResult.UserFound;

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any())
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Role.MissingRole });
            }

            var roleName = roles.First();

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                return _serviceResultFactory.GeneralOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }

            return _serviceResultFactory.GeneralOperationSuccess();
        }

        private async Task<bool> DoesRoleExist(string roleName)
        {
            return await _roleManager.RoleExistsAsync(roleName);
        }

        private static bool IsUserActive(User user)
        {
            return user.AccountStatus == 1;
        }
    }
}
