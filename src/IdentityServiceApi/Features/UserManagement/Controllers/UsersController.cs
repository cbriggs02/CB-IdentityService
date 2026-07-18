using Asp.Versioning;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.DTOs;
using IdentityServiceApi.Features.UserManagement.Models.Requests;
using IdentityServiceApi.Features.UserManagement.Models.Responses;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Models;
using IdentityServiceApi.Shared.Results;
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
            var result = await userService.GetUsersAsync(request);
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
        /// <response code="500">
        ///     The server encountered an unexpected error while processing the request.
        /// </response>
        [Authorize(Roles = RoleGroups.AllStandardRoles)]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.GetUserById)]
        public async Task<ActionResult<UserResponse>> GetUserAsync([FromRoute][Required] string id)
        {
            var result = await userService.GetUserAsync(id);
            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.Forbidden => Forbid(),
                    ErrorType.NotFound => NotFound(),
                    _ => StatusCode(500, new ErrorResponse
                    {
                        Errors = [ErrorMessages.General.GlobalExceptionMessage]
                    })
                };
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
        /// <response code="422">
        ///     The request was well-formed but could not be processed due to invalid country id.
        /// </response>
        /// <response code="500">
        ///     The server encountered an unexpected error while processing the request.
        /// </response>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.CreateUser)]
        public async Task<ActionResult<UserResponse>> CreateUserAsync([FromBody] UserDTO user)
        {
            var result = await userService.CreateUserAsync(user);
            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.UnprocessableEntity => UnprocessableEntity(new ErrorResponse
                    {
                        Errors = result.Errors
                    }),
                    ErrorType.Validation => BadRequest(new ErrorResponse
                    {
                        Errors = result.Errors
                    }),
                    _ => StatusCode(500, new ErrorResponse
                    {
                        Errors = [ErrorMessages.General.GlobalExceptionMessage]
                    })
                };
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
        ///     The user is accessing a resource with improper permissions.
        /// </response>
        /// <response code="404">
        ///     The user was not found.
        /// </response>
        /// <response code="422">
        ///     The request was well-formed but could not be processed due to invalid country id.
        /// </response>
        /// <response code="500">
        ///     The server encountered an unexpected error while processing the request.
        /// </response>
        [Authorize(Roles = RoleGroups.AllStandardRoles)]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.UpdateUser)]
        public async Task<IActionResult> UpdateUserAsync([FromRoute][Required] string id, [FromBody] UserDTO user)
        {
            var result = await userService.UpdateUserAsync(id, user);
            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.Forbidden => Forbid(),
                    ErrorType.NotFound => NotFound(),
                    ErrorType.UnprocessableEntity => UnprocessableEntity(new ErrorResponse
                    {
                        Errors = result.Errors
                    }),
                    ErrorType.Validation => BadRequest(new ErrorResponse
                    {
                        Errors = result.Errors
                    }),
                    _ => StatusCode(500, new ErrorResponse
                    {
                        Errors = [ErrorMessages.General.GlobalExceptionMessage]
                    })
                };
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
        /// <response code="500">
        ///     The server encountered an unexpected error while processing the request.
        /// </response>
        [Authorize(Roles = RoleGroups.AllStandardRoles)]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        [SwaggerOperation(Summary = ApiDocumentation.UsersApi.DeleteUser)]
        public async Task<IActionResult> DeleteUserAsync([FromRoute][Required] string id)
        {
            var result = await userService.DeleteUserAsync(id);
            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.Forbidden => Forbid(),
                    ErrorType.NotFound => NotFound(),
                    ErrorType.Validation => BadRequest(new ErrorResponse
                    {
                        Errors = result.Errors
                    }),
                    _ => StatusCode(500, new ErrorResponse
                    {
                        Errors = [ErrorMessages.General.GlobalExceptionMessage]
                    })
                };
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
        /// <response code="403">
        ///     The user is not authorized.
        /// </response>
        /// <response code="404">
        ///     The user was not found.
        /// </response>
        /// <response code="409">
        ///     The request could not be completed due to a conflict with the current state of the resource.
        /// </response>
        /// <response code="500">
        ///     The server encountered an unexpected error while processing the request.
        /// </response>
        [Authorize(Roles = RoleGroups.AdminOnly)]
        [HttpPatch("activate/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> ActivateUserAsync([FromRoute][Required] string id)
        {
            var result = await userService.ActivateUserAsync(id);
            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.Forbidden => Forbid(),
                    ErrorType.NotFound => NotFound(),
                    ErrorType.InvalidState => Conflict(new ErrorResponse
                    {
                        Errors = result.Errors
                    }),
                    ErrorType.Validation => BadRequest(new ErrorResponse
                    {
                        Errors = result.Errors
                    }),
                    _ => StatusCode(500, new ErrorResponse
                    {
                        Errors = [ErrorMessages.General.GlobalExceptionMessage]
                    })
                };
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
        /// <response code="403">
        ///     The user is not authorized.
        /// </response>
        /// <response code="404">
        ///     The user was not found.
        /// </response>
        /// <response code="409">
        ///     The request could not be completed due to a conflict with the current state of the resource.
        /// </response>
        /// <response code="500">
        ///     The server encountered an unexpected error while processing the request.
        /// </response>
        [Authorize(Roles = RoleGroups.AdminOnly)]
        [HttpPatch("deactivate/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> DeactivateUserAsync([FromRoute][Required] string id)
        {
            var result = await userService.DeactivateUserAsync(id);
            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.Forbidden => Forbid(),
                    ErrorType.NotFound => NotFound(),
                    ErrorType.InvalidState => Conflict(new ErrorResponse
                    {
                        Errors = result.Errors
                    }),
                    ErrorType.Validation => BadRequest(new ErrorResponse
                    {
                        Errors = result.Errors
                    }),
                    _ => StatusCode(500, new ErrorResponse
                    {
                        Errors = [ErrorMessages.General.GlobalExceptionMessage]
                    })
                };
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
        /// <response code="404">
        ///     The user was not found.
        /// </response>
        /// <response code="409">
        ///     The request could not be completed due to a conflict with the current state of the resource.
        /// </response>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPost("{id}/roles")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> AssignRoleAsync([FromRoute][Required] string id, [FromBody][Required] string roleName)
        {
            var result = await userService.AssignRoleAsync(id, roleName);
            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(),
                    ErrorType.InvalidState => Conflict(new ErrorResponse
                    {
                        Errors = result.Errors
                    }),
                    ErrorType.Validation => BadRequest(new ErrorResponse
                    {
                        Errors = result.Errors
                    }),
                    _ => BadRequest(new ErrorResponse
                    {
                        Errors = [ErrorMessages.General.GlobalExceptionMessage]
                    })
                };
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
        /// <response code="404">
        ///     The user was not found.
        /// </response>
        /// <response code="409">
        ///     The request could not be completed due to a conflict with the current state of the resource.
        /// </response>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpDelete("{id}/roles")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> RemoveRoleAsync([FromRoute][Required] string id)
        {
            var result = await userService.RemoveRoleAsync(id);
            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(),
                    ErrorType.InvalidState => Conflict(new ErrorResponse
                    {
                        Errors = result.Errors
                    }),
                    ErrorType.Validation => BadRequest(new ErrorResponse
                    {
                        Errors = result.Errors
                    }),
                    _ => BadRequest(new ErrorResponse
                    {
                        Errors = [ErrorMessages.General.GlobalExceptionMessage]
                    })
                };
            }
            return NoContent();
        }
    }
}
