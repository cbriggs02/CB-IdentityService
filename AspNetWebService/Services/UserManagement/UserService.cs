﻿using AspNetWebService.Constants;
using AspNetWebService.Interfaces.Authorization;
using AspNetWebService.Interfaces.UserManagement;
using AspNetWebService.Interfaces.Utilities;
using AspNetWebService.Models.DataTransferObjectModels;
using AspNetWebService.Models.Entities;
using AspNetWebService.Models.PaginationModels;
using AspNetWebService.Models.RequestModels.UserManagement;
using AspNetWebService.Models.ServiceResultModels.Common;
using AspNetWebService.Models.ServiceResultModels.UserManagement;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetWebService.Services.UserManagement
{
    /// <summary>
    ///     Service responsible for interacting with user-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IPasswordHistoryService _passwordHistoryService;
        private readonly IPermissionService _permissionService;
        private readonly IParameterValidator _parameterValidator;
        private readonly IServiceResultFactory _serviceResultFactory;
        private readonly IUserLookupService _userLookupService;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userManager">
        ///     The user manager responsible for handling user management operations.
        /// </param>
        /// <param name="passwordHistoryService">
        ///     Service for managing password history, such as removing old passwords after a user is deleted.
        /// </param>
        /// <param name="permissionService">
        ///     Service for validating and checking user permissions within the system.
        /// </param>
        /// <param name="parameterValidator">
        ///     The paramter validator service used for defense checking service paramters.
        /// </param>
        /// <param name="serviceResultFactory">
        ///     The service used for creating the result objects being returned in operations.
        /// </param>
        /// <param name="userLookupService">'
        ///     The service used for looking up users in the system.
        /// </param>
        /// <param name="mapper">
        ///     Object mapper for converting between entities and data transfer objects (DTOs).
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when any of the provided service parameters are null.
        /// </exception>
        public UserService(UserManager<User> userManager, IPasswordHistoryService passwordHistoryService, IPermissionService permissionService, IParameterValidator parameterValidator, IServiceResultFactory serviceResultFactory, IUserLookupService userLookupService, IMapper mapper)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _passwordHistoryService = passwordHistoryService ?? throw new ArgumentNullException(nameof(passwordHistoryService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
            _serviceResultFactory = serviceResultFactory ?? throw new ArgumentNullException(nameof(serviceResultFactory));
            _userLookupService = userLookupService ?? throw new ArgumentNullException(nameof(userLookupService));
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
        public async Task<UserServiceListResult> GetUsers(UserListRequest request)
        {
            _parameterValidator.ValidateObjectNotNull(request, nameof(request));

            var query = _userManager.Users.AsQueryable();

            if (request.AccountStatus.HasValue)
            {
                query = query.Where(user => user.AccountStatus == request.AccountStatus.Value);
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(user => user.LastName)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .AsNoTracking()
                .ToListAsync();

            var userDTOs = users.Select(user => _mapper.Map<UserDTO>(user)).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            PaginationMetadata paginationMetadata = new()
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
        public async Task<UserServiceResult> GetUser(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var permissionResult = await _permissionService.ValidatePermissions(id);

            if (!permissionResult.Success)
            {
                return _serviceResultFactory.UserOperationFailure(permissionResult.Errors.ToArray());
            }

            var userLookupResult = await _userLookupService.FindUserById(id);

            if(!userLookupResult.Success)
            {
                return _serviceResultFactory.UserOperationFailure(userLookupResult.Errors.ToArray());
            }

            var user = userLookupResult.UserFound;
            var userDTO = _mapper.Map<UserDTO>(user);

            return _serviceResultFactory.UserOperationSuccess(userDTO);
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
        public async Task<UserServiceResult> CreateUser(UserDTO user)
        {
            ValidateUserDTO(user);

            var newUser = new User
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Country = user.Country,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            var result = await _userManager.CreateAsync(newUser);

            if (result.Succeeded)
            {
                var returnUser = new UserDTO
                {
                    UserName = newUser.UserName,
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    Email = newUser.Email,
                    PhoneNumber = newUser.PhoneNumber,
                    Country = newUser.Country
                };

                return _serviceResultFactory.UserOperationSuccess(returnUser);
            }
            else
            {
                return _serviceResultFactory.UserOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }
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
        public async Task<ServiceResult> UpdateUser(string id, UserDTO user)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            ValidateUserDTO(user);

            var permissionResult = await _permissionService.ValidatePermissions(id);

            if (!permissionResult.Success)
            {
                return _serviceResultFactory.GeneralOperationFailure(permissionResult.Errors.ToArray());
            }

            var userLookupResult = await _userLookupService.FindUserById(id);

            if(!userLookupResult.Success)
            {
                return _serviceResultFactory.GeneralOperationFailure(userLookupResult.Errors.ToArray());
            }

            var existingUser = userLookupResult.UserFound;

            existingUser.UserName = user.UserName;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Country = user.Country;
            existingUser.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(existingUser);

            if (!result.Succeeded)
            {
                return _serviceResultFactory.GeneralOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }

            return _serviceResultFactory.GeneralOperationSuccess();
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
        public async Task<ServiceResult> DeleteUser(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var permissionResult = await _permissionService.ValidatePermissions(id);

            if (!permissionResult.Success)
            {
                return _serviceResultFactory.GeneralOperationFailure(permissionResult.Errors.ToArray());
            }

            var userLookupServiceResult = await _userLookupService.FindUserById(id);

            if (!userLookupServiceResult.Success)
            {
                return _serviceResultFactory.UserOperationFailure(userLookupServiceResult.Errors.ToArray());
            }

            var user = userLookupServiceResult.UserFound;

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                return _serviceResultFactory.GeneralOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }

            // delete all stored passwords for user once user is deleted for data clean up.
            await _passwordHistoryService.DeletePasswordHistory(id);
            return _serviceResultFactory.GeneralOperationSuccess();
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
        public async Task<ServiceResult> ActivateUser(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var permissionResult = await _permissionService.ValidatePermissions(id);

            if (!permissionResult.Success)
            {
                return _serviceResultFactory.GeneralOperationFailure(permissionResult.Errors.ToArray());
            }

            var userLookupServiceResult = await _userLookupService.FindUserById(id);

            if (!userLookupServiceResult.Success)
            {
                return _serviceResultFactory.UserOperationFailure(userLookupServiceResult.Errors.ToArray());
            }

            var user = userLookupServiceResult.UserFound;

            if (user.AccountStatus == 1)
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.User.AlreadyActivated });
            }

            user.AccountStatus = 1;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return _serviceResultFactory.GeneralOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }

            return _serviceResultFactory.GeneralOperationSuccess();
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
        public async Task<ServiceResult> DeactivateUser(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var permissionResult = await _permissionService.ValidatePermissions(id);

            if (!permissionResult.Success)
            {
                return _serviceResultFactory.GeneralOperationFailure(permissionResult.Errors.ToArray());
            }

            var userLookupServiceResult = await _userLookupService.FindUserById(id);

            if (!userLookupServiceResult.Success)
            {
                return _serviceResultFactory.UserOperationFailure(userLookupServiceResult.Errors.ToArray());
            }

            var user = userLookupServiceResult.UserFound;

            if (user.AccountStatus == 0)
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.User.NotActivated });
            }

            user.AccountStatus = 0;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return _serviceResultFactory.GeneralOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }

            return _serviceResultFactory.GeneralOperationSuccess();
        }


        /// <summary>
        ///     Validates the properties of the provided <see cref="UserDTO"/> object to ensure
        ///     it is properly initialized and contains all required data.
        /// </summary>
        /// <param name="user">
        ///     The <see cref="UserDTO"/> instance to validate. Expected to contain
        ///     values for properties such as UserName, FirstName, LastName, Email, PhoneNumber, and Country.
        /// </param>
        private void ValidateUserDTO(UserDTO user)
        {
            _parameterValidator.ValidateObjectNotNull(user, nameof(user));
            _parameterValidator.ValidateNotNullOrEmpty(user.UserName, nameof(user.UserName));
            _parameterValidator.ValidateNotNullOrEmpty(user.FirstName, nameof(user.FirstName));
            _parameterValidator.ValidateNotNullOrEmpty(user.LastName, nameof(user.LastName));
            _parameterValidator.ValidateNotNullOrEmpty(user.Email, nameof(user.Email));
            _parameterValidator.ValidateNotNullOrEmpty(user.PhoneNumber, nameof(user.PhoneNumber));
            _parameterValidator.ValidateNotNullOrEmpty(user.Country, nameof(user.Country));
        }
    }
}
