using Asp.Versioning;
using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Models.ApiResponseModels.Shared;
using IdentityServiceApi.Models.ApiResponseModels.Users;
using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.RequestModels.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace IdentityServiceApi.Controllers
{
    /// <summary>
    ///     
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
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
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
        ///    
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
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
            return CreatedAtAction("GetUser", new { id = response.User.Id }, response.User);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = RoleGroups.AdminOnly)]
        [HttpPatch("activate/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = RoleGroups.AdminOnly)]
        [HttpPatch("deactivate/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPost("{id}/roles")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpDelete("{id}/roles")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
