using Asp.Versioning;
using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Authorization;
using IdentityServiceApi.Models.ApiResponseModels.Shared;
using IdentityServiceApi.Models.ApiResponseModels.RolesResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace IdentityServiceApi.Controllers
{
    /// <summary>
    ///     Controller for handling API operations related to roles.
    ///     This controller processes all incoming requests related to role management and delegates
    ///     them to the role service, which implements the business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[Controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RolesController"/> 
        ///     class with the specified dependencies.
        /// </summary>
        /// <param name="roleService">
        ///     roles service used for all role-related operations.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public RolesController(IRoleService roleService)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        }

        /// <summary>
        ///     Asynchronously processes requests for retrieving a list of all roles in the system,
        ///     delegating the operation to the required service.
        /// </summary>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) with a list of identity roles.    
        ///     - <see cref="StatusCodes.Status204NoContent"/> (No Content) if no roles are found in the system.  
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user who 
        ///         is not authenticated or does not have the required role.
        /// </returns>
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RolesListResponse))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(Summary = ApiDocumentation.RolesApi.GetRoles)]
        public async Task<ActionResult<RolesListResponse>> GetRolesAsync()
        {
            var result = await _roleService.GetRolesAsync();

            if (result.Roles == null || !result.Roles.Any())
            {
                return NoContent();
            }

            return Ok(new RolesListResponse { Roles = result.Roles });
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
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the role assignment was successful.    
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors 
        ///         returned by the role service that occurred during the role assignment.         
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made 
        ///         by a user who is not authenticated or does not have the required role.    
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        /// </returns>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("users/{id}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.RolesApi.AssignRole)]
        public async Task<IActionResult> AssignRoleAsync([FromRoute][Required] string id, [FromBody][Required(ErrorMessage = "Role Name is required.")] string roleName)
        {
            var result = await _roleService.AssignRoleAsync(id, roleName);

            if (!result.Success)
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                return BadRequest(new ErrorResponse { Errors = result.Errors });
            }

            return Ok();
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
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the role removal was successful.     
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors 
        ///         returned by the role service that occurred when removing the role.       
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made 
        ///         by a user who is not authenticated or does not have the required role.   
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        /// </returns>
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("users/{id}/roles/{roleName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.RolesApi.RemoveRole)]
        public async Task<IActionResult> RemoveRoleAsync([FromRoute][Required] string id, [FromRoute][Required] string roleName)
        {
            var result = await _roleService.RemoveRoleAsync(id, roleName);

            if (!result.Success)
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                return BadRequest(new ErrorResponse { Errors = result.Errors });
            }

            return Ok();
        }
    }
}
