using Microsoft.AspNetCore.Mvc;
using IdentityServiceApi.Models.DTO;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using IdentityServiceApi.Constants;
using IdentityServiceApi.Models.ApiResponseModels.Users;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Models.ApiResponseModels.Shared;
using Asp.Versioning;
using IdentityServiceApi.Models.RequestModels.UserManagement;

namespace IdentityServiceApi.Controllers
{
    /// <summary>
    ///     Controller for handling API operations related to users.
    ///     This controller processes all incoming requests related to user management and delegates
    ///     them to the user service, which implements the business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UsersController"/> class with the specified dependencies.
        /// </summary>
        /// <param name="userService">
        ///     User service used for all user-related operations.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public UsersController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        /// <summary>
        ///     Asynchronously processes requests to retrieve a paginated list of users from the system, based on 
        ///     the provided page number and page size, optional account status for filtering and delegates 
        ///     to the required service.
        /// </summary>
        /// <param name="request">
        ///     A model containing pagination details, such as the page number and page size
        ///     and account status for optional filtering.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) with a list of user DTO objects and pagination 
        ///         metadata in headers ("X-Pagination"). 
        ///     - <see cref="StatusCodes.Status204NoContent"/> (No Content) if no users are found for the specified page.    
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user 
        ///         who is not authenticated.
        ///     - <see cref="StatusCodes.Status403"/> (Forbidden) if the request is made by a user 
        ///         who has insufficient privileges.
        ///     - <see cref="StatusCodes.Status500InternalServerError"/> (Internal Server Error) if an unexpected error occurs.       
        /// </returns>
        [Authorize(Roles = RoleGroups.AdminOnly)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserListResponse))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.GetUsers)]
        public async Task<ActionResult<UserListResponse>> GetUsersAsync([FromQuery] UserListRequest request)
        {
            var result = await _userService.GetUsersAsync(request);
            if (result.Users == null || !result.Users.Any())
            {
                return NoContent();
            }

            var response = new UserListResponse
            {
                Users = result.Users,
                PaginationMetadata = result.PaginationMetadata
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(response.PaginationMetadata));
            return Ok(response);
        }

        /// <summary>
        ///     Asynchronously processes requests for retrieving a user from the system by the provided ID, 
        ///     delegating the operation to the appropriate service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user to retrieve.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) with a user DTO object if the user is found.   
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user
        ///         who is not authenticated or does not have the required role.    
        ///     - <see cref="StatusCodes.Status403Forbidden"/> (Forbidden) if an authorized user tries to retrieve
        ///         another user's account or admin tries to retrieve another admins account.   
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the specified user is not found.
        ///     - <see cref="StatusCodes.Status500InternalServerError"/> (Internal Server Error) if an unexpected error occurs.       
        /// </returns>
        [Authorize(Roles = RoleGroups.AllStandardRoles)]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.GetUserById)]
        public async Task<ActionResult<UserResponse>> GetUserAsync([FromRoute][Required] string id)
        {
            var result = await _userService.GetUserAsync(id);
            if (!result.Success)
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.Authorization.Forbidden, StringComparison.OrdinalIgnoreCase)))
                {
                    return Forbid();
                }

                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }
            }

            return Ok(new UserResponse { User = result.User });
        }

        /// <summary>
        ///     Asynchronously retrieves aggregated metrics representing the number of users created on each date.
        /// </summary>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) with a <see cref="UserCreationStatsResponse"/> containing user creation date metrics.
        ///     - <see cref="StatusCodes.Status204NoContent"/> (No Content) if no user creation data is available.
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user who is not 
        ///         authenticated.
        ///     - <see cref="StatusCodes.Status403"/> (Forbidden) if the request is made by a user 
        ///         who has insufficient privileges.
        ///     - <see cref="StatusCodes.Status500InternalServerError"/> (Internal Server Error) if an unexpected error occurs.       
        /// </returns>
        [Authorize(Roles = RoleGroups.AdminOnly)]
        [HttpGet("creation-stats")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserCreationStatsResponse))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.GetUserCreationStats)]
        public async Task<ActionResult<UserCreationStatsResponse>> GetUserCreationStatsAsync()
        {
            var result = await _userService.GetUserCreationStatsAsync();
            if (result == null || !result.UserCreationStats.Any())
            {
                return NoContent();
            }

            return Ok(new UserCreationStatsResponse { UserCreationStats = result.UserCreationStats });
        }

        /// <summary>
        ///     Asynchronously retrieves aggregated metrics for user states, including total, activated, and deactivated users.
        /// </summary>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) with a <see cref="UserStateMetricsResponse"/> containing user state metrics.
        ///     - <see cref="StatusCodes.Status204NoContent"/> (No Content) if no user data is available.
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user who is not 
        ///			authenticated.
        ///     - <see cref="StatusCodes.Status403"/> (Forbidden) if the request is made by a user 
        ///         who has insufficient privileges.
        ///     - <see cref="StatusCodes.Status500InternalServerError"/> (Internal Server Error) if an unexpected error occurs.       
        /// </returns>
        [Authorize(Roles = RoleGroups.AdminOnly)]
		[HttpGet("state-metrics")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserStateMetricsResponse))]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[SwaggerOperation(Summary = ApiDocumentation.UsersApi.GetUserStateMetrics)]
		public async Task<ActionResult<UserStateMetricsResponse>> GetUserStateMetricsAsync()
		{
			var result = await _userService.GetUserStateMetricsAsync();
			if (result == null)
			{
				return NoContent();
			}

			return Ok(new UserStateMetricsResponse
			{
				TotalCount = result.TotalCount,
				ActivatedUsers = result.ActivatedUsers,
				DeactivatedUsers = result.DeactivatedUsers
			});
		}

        /// <summary>
        ///     Asynchronously processes requests for creating a new user in the system, delegating the operation 
        ///     to the required service.
        /// </summary>
        /// <param name="user">
        ///     The UserDTO object containing the new user's information.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status201Created"/> (Created) with a User DTO of the newly created user.    
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of validation or creation 
        ///			errors encountered during user creation.
        ///     - <see cref="StatusCodes.Status500InternalServerError"/> (Internal Server Error) if an unexpected error occurs.       
        /// </returns>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.CreateUser)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponse>> CreateUserAsync([FromBody] UserDTO user)
        {
            var result = await _userService.CreateUserAsync(user);
            if (!result.Success)
            {
                return BadRequest(new ErrorResponse { Errors = result.Errors });
            }

            var response = new UserResponse { User = result.User };
            return CreatedAtAction("GetUser", new { id = response.User.Id }, response.User);
        }

        /// <summary>
        ///     Asynchronously processes requests for updating a user account in the system by the provided ID,
        ///     delegating the operation to the required service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user account to update.
        /// </param>
        /// <param name="user">
        ///     The UserDTO object containing the updated user information.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status204NoContent"/> (No Content) if the user account was successfully updated.    
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of validation or update errors 
        ///         encountered during the operation.   
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user who is not 
        ///         authenticated or does not have the required role.    
        ///     - <see cref="StatusCodes.Status403Forbidden"/> (Forbidden) if an authorized user attempts to update another 
        ///         user's account or a admin attempts to update another admins account.
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the specified user account is not found.
        ///     - <see cref="StatusCodes.Status500InternalServerError"/> (Internal Server Error) if an unexpected error occurs.       
        /// </returns>
        [Authorize(Roles = RoleGroups.AllStandardRoles)]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.UpdateUser)]
        public async Task<IActionResult> UpdateUserAsync([FromRoute][Required] string id, [FromBody] UserDTO user)
        {
            var result = await _userService.UpdateUserAsync(id, user);
            if (!result.Success)
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.Authorization.Forbidden, StringComparison.OrdinalIgnoreCase)))
                {
                    return Forbid();
                }

                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                return BadRequest(new ErrorResponse { Errors = result.Errors });
            }

            return NoContent();
        }

        /// <summary>
        ///     Asynchronously processes requests for deleting a user account in the system by the provided ID,
        ///     delegating the operation to the required service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user account to delete.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status204NoContent"/> (NoContent) if the user account deletion was successful.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors encountered during 
        ///         the user account deletion.   
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user who is 
        ///         not authenticated or does not have the required role.    
        ///     - <see cref="StatusCodes.Status403Forbidden"/> (Forbidden) if an authorized user attempts to delete another 
        ///         user's account or a admin tries to delete another admin account.     
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the specified user account is not found.
        ///     - <see cref="StatusCodes.Status500InternalServerError"/> (Internal Server Error) if an unexpected error occurs.       
        /// </returns>
        [Authorize(Roles = RoleGroups.AllStandardRoles)]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.DeleteUser)]
        public async Task<IActionResult> DeleteUserAsync([FromRoute][Required] string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result.Success)
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.Authorization.Forbidden, StringComparison.OrdinalIgnoreCase)))
                {
                    return Forbid();
                }

                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                return BadRequest(new ErrorResponse { Errors = result.Errors });
            }

            return NoContent();
        }

        /// <summary>
        ///     Asynchronously processes requests for activating a user account in the system by the provided ID,
        ///     delegating the operation to the required service.
        /// </summary>
        /// <param name="id">
        ///     The identifier of the user account to activate.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status204NoContent"/> (NoContent) if the user account activation was successful.  
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors encountered
        ///         during the user account activation.    
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user 
        ///         who is not authenticated or does not have the required role.
        ///     - <see cref="StatusCodes.Status403Forbidden"/> (Forbidden) if an authorized user attempts to activate another 
        ///         user's account or a admin tries to activate another admin account.         
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the specified user account is not found.
        ///     - <see cref="StatusCodes.Status500InternalServerError"/> (Internal Server Error) if an unexpected error occurs.       
        /// </returns>
        [Authorize(Roles = RoleGroups.AdminOnly)]
        [HttpPatch("activate/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.ActivateUser)]
        public async Task<IActionResult> ActivateUserAsync([FromRoute][Required] string id)
        {
            var result = await _userService.ActivateUserAsync(id);
            if (!result.Success)
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.Authorization.Forbidden, StringComparison.OrdinalIgnoreCase)))
                {
                    return Forbid();
                }

                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                return BadRequest(new ErrorResponse { Errors = result.Errors });
            }

            return NoContent();
        }

        /// <summary>
        ///     Asynchronously processes requests for deactivating a user account in the system by the provided ID,
        ///     delegating the operation to the required service.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user account to deactivate.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status204NoContent"/> (NoContent) if the user account deactivation was successful. 
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors encountered 
        ///         during the user account deactivation. 
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user who 
        ///         is not authenticated or does not have the required role.
        ///     - <see cref="StatusCodes.Status403Forbidden"/> (Forbidden) if an authorized user attempts to deactivate another 
        ///         user's account or a admin tries to deactivate another admin account.    
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the specified user account is not found.
        ///     - <see cref="StatusCodes.Status500InternalServerError"/> (Internal Server Error) if an unexpected error occurs.       
        /// </returns>
        [Authorize(Roles = RoleGroups.AdminOnly)]
        [HttpPatch("deactivate/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.DeactivateUser)]
        public async Task<IActionResult> DeactivateUserAsync([FromRoute][Required] string id)
        {
            var result = await _userService.DeactivateUserAsync(id);
            if (!result.Success)
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.Authorization.Forbidden, StringComparison.OrdinalIgnoreCase)))
                {
                    return Forbid();
                }

                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                return BadRequest(new ErrorResponse { Errors = result.Errors });
            }

            return NoContent();
        }

        /// <summary>
        ///     Asynchronously processes requests for assigning a role to a user in the system
        ///     by delegating the operation to the required service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user to whom the role is being assigned.
        /// </param>
        /// <param name="roleName">
        ///     The name of the role being assigned to the user.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status204NoContent"/> (NoContent) if the role assignment was successful.    
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors 
        ///         returned by the role service that occurred during the role assignment.         
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made 
        ///         by a user who is not authenticated.    
        ///     - <see cref="StatusCodes.Status403"/> (Forbidden) if the request is made by a user 
        ///         who has insufficient privileges.
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        ///     - <see cref="StatusCodes.Status500InternalServerError"/> (Internal Server Error) if an unexpected error occurs.       
        /// </returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPost("{id}/roles")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.AssignRole)]
        public async Task<IActionResult> AssignRoleAsync([FromRoute][Required] string id, [FromBody][Required(ErrorMessage = "Role Name is required.")] string roleName)
        {
            var result = await _userService.AssignRoleAsync(id, roleName);
            if (!result.Success)
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                return BadRequest(new ErrorResponse { Errors = result.Errors });
            }

            return NoContent();
        }

        /// <summary>
        ///     Asynchronously processes requests for removing a role from a user in the system
        ///     by delegating the operation to the required service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user to whom the role is being removed.
        /// </param>
        /// <param name="roleName">
        ///     The name of the role being removed to the user.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status204NoContent"/> (NoContent) if the role removal was successful.     
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors 
        ///         returned by the role service that occurred when removing the role.       
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made 
        ///         by a user who is not authenticated.   
        ///     - <see cref="StatusCodes.Status403"/> (Forbidden) if the request is made by a user 
        ///         who has insufficient privileges.
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        ///     - <see cref="StatusCodes.Status500InternalServerError"/> (Internal Server Error) if an unexpected error occurs.       
        /// </returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpDelete("{id}/roles")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.RemoveRole)]
        public async Task<IActionResult> RemoveAssignedRoleAsync([FromRoute][Required] string id)
        {
            var result = await _userService.RemoveAssignedRoleAsync(id);
            if (!result.Success)
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                return BadRequest(new ErrorResponse { Errors = result.Errors });
            }

            return NoContent();
        }
    }
}
