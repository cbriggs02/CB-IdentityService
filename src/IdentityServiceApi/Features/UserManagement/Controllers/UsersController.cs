using Asp.Versioning;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.DTOs;
using IdentityServiceApi.Features.UserManagement.Models.Requests;
using IdentityServiceApi.Features.UserManagement.Models.Responses;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace IdentityServiceApi.Features.UserManagement.Controllers
{
    /// <summary>
    ///     Provides endpoints for managing users, including creation, retrieval, updates, and role assignments.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2024  
    ///     @Updated: 2026  
    /// </remarks>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users")]
    public class UsersController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

        /// <summary>
        ///     Retrieves a paginated list of users based on the provided query parameters.
        /// </summary>
        /// <param name="request">
        ///     The request containing filtering and pagination criteria.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="UserListResponse"/> containing users and pagination metadata.
        /// </returns>
        /// <response code="200">
        ///     Users were successfully retrieved.
        /// </response>
        /// <response code="204">
        ///     No users were found.
        /// </response>
        [Authorize(Roles = RoleGroups.AdminOnly)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserListResponse))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
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

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(response.PaginationMetadata));
            return Ok(response);
        }

        /// <summary>
        ///     Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="UserResponse"/> containing the user details.
        /// </returns>
        /// <response code="200">
        ///     The user was successfully retrieved.
        /// </response>
        /// <response code="403">
        ///     The user is not authorized to perform this action.
        /// </response>
        /// <response code="404">
        ///     The user could not be found.
        /// </response>
        [Authorize(Roles = RoleGroups.AllStandardRoles)]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        ///     Creates a new user in the system.
        /// </summary>
        /// <param name="user">
        ///     The user data used to create a new user.
        /// </param>
        /// <returns>
        ///     Returns the created user.
        /// </returns>
        /// <response code="201">
        ///     The user was successfully created.
        /// </response>
        /// <response code="400">
        ///     The request was invalid or failed validation.
        /// </response>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.CreateUser)]
        public async Task<ActionResult<UserResponse>> CreateUserAsync([FromBody] UserDTO user)
        {
            var result = await _userService.CreateUserAsync(user);
            if (!result.Success)
            {
                return BadRequest(new ErrorResponse { Errors = result.Errors });
            }

            var response = new UserResponse { User = result.User };
            return CreatedAtAction(nameof(GetUserAsync), new { id = response.User.Id }, response.User);
        }

        /// <summary>
        ///     Updates an existing user's information.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user.
        /// </param>
        /// <param name="user">
        ///     The updated user data.
        /// </param>
        /// <returns>
        ///     Status indicating the outcome of the operation.
        /// </returns>
        /// <response code="204">
        ///     The user was successfully updated.
        /// </response>
        /// <response code="400">
        ///     The request was invalid.
        /// </response>
        /// <response code="403">
        ///     The user is not authorized.
        /// </response>
        /// <response code="404">
        ///     The user was not found.
        /// </response>
        [Authorize(Roles = RoleGroups.AllStandardRoles)]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        ///     Deletes a user from the system.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user.
        /// </param>
        /// <returns>
        ///     Status indicating the outcome of the operation.
        /// returns>
        /// <response code="204">
        ///     The user was successfully deleted.
        /// </response>
        /// <response code="400">
        ///     The request failed validation.
        /// </response>
        /// <response code="403">
        ///     The user is not authorized.
        /// </response>
        /// <response code="404">
        ///     The user was not found.
        /// </response>
        [Authorize(Roles = RoleGroups.AllStandardRoles)]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        ///     Activates a user account.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to activate.
        /// </param>
        /// <returns>
        ///     Returns a status indicating the outcome of the activation operation.
        /// </returns>
        /// <response code="204">
        ///     The user was successfully activated.
        /// </response>
        /// <response code="400">
        ///     The request failed due to validation or business logic errors.
        /// </response>
        [Authorize(Roles = RoleGroups.AdminOnly)]
        [HttpPatch("activate/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> ActivateUserAsync([FromRoute][Required] string id)
        {
            var result = await _userService.ActivateUserAsync(id);
            if (!result.Success)
            {
                return BadRequest(new ErrorResponse { Errors = result.Errors });
            }

            return NoContent();
        }

        /// <summary>
        ///     Activates a user account.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to activate.
        /// </param>
        /// <returns>
        ///     Returns a status indicating the outcome of the activation operation.
        /// </returns>
        /// <response code="204">
        ///     The user was successfully activated.
        /// </response>
        /// <response code="400">
        ///     The request failed due to validation or business logic errors.
        /// </response>
        [Authorize(Roles = RoleGroups.AdminOnly)]
        [HttpPatch("deactivate/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> DeactivateUserAsync([FromRoute][Required] string id)
        {
            var result = await _userService.DeactivateUserAsync(id);
            if (!result.Success)
            {
                return BadRequest(new ErrorResponse { Errors = result.Errors });
            }

            return NoContent();
        }

        /// <summary>
        ///     Activates a user account.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to activate.
        /// </param>
        /// <returns>
        ///     Returns a status indicating the outcome of the activation operation.
        /// </returns>
        /// <response code="204">
        ///     The user was successfully activated.
        /// </response>
        /// <response code="400">
        ///     The request failed due to validation or business logic errors.
        /// </response>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPost("{id}/roles")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> AssignRoleAsync([FromRoute][Required] string id, [FromBody][Required] string roleName)
        {
            var result = await _userService.AssignRoleAsync(id, roleName);
            if (!result.Success)
            {
                return BadRequest(new ErrorResponse { Errors = result.Errors });
            }

            return NoContent();
        }

        /// <summary>
        ///     Activates a user account.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to activate.
        /// </param>
        /// <returns>
        ///     Returns a status indicating the outcome of the activation operation.
        /// </returns>
        /// <response code="204">
        ///     The user was successfully activated.
        /// </response>
        /// <response code="400">
        ///     The request failed due to validation or business logic errors.
        /// </response>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpDelete("{id}/roles")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> RemoveRoleAsync([FromRoute][Required] string id)
        {
            var result = await _userService.RemoveRoleAsync(id);
            if (!result.Success)
            {
                return BadRequest(new ErrorResponse { Errors = result.Errors });
            }

            return NoContent();
        }
    }
}
