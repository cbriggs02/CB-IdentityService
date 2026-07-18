using Asp.Versioning;
using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Features.Authorization.Models;
using IdentityServiceApi.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace IdentityServiceApi.Features.Authorization.Controllers
{
    /// <summary>
    ///     Provides endpoints for managing and retrieving role information.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2024  
    ///     @Updated: 2026  
    /// </remarks>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/roles")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class RolesController(IRoleService roleService) : ControllerBase
    {
        /// <summary>
        ///     Retrieves all roles in the system.
        /// </summary>
        /// <returns>
        ///     Returns a <see cref="RolesListResponse"/> containing a collection of roles.
        /// </returns>
        /// <response code="200">
        ///     A list of roles was successfully retrieved.
        /// </response>
        /// <response code="204">
        ///     No roles were found in the system.
        /// </response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RolesListResponse))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation(Summary = ApiDocumentation.RolesApi.GetRoles)]
        public async Task<ActionResult<RolesListResponse>> GetRolesAsync()
        {
            var result = await roleService.GetRolesAsync();
            if (result.Roles == null || !result.Roles.Any())
            {
                return NoContent();
            }

            return Ok(new RolesListResponse { Roles = result.Roles });
        }

        /// <summary>
        ///     Retrieves a specific role by its unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the role to retrieve.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="RoleResponse"/> containing the requested role.
        /// </returns>
        /// <response code="200">
        ///     The role was successfully retrieved.
        /// </response>
        /// <response code="404">
        ///     The specified role could not be found.
        /// </response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.RolesApi.GetRoleById)]
        public async Task<ActionResult<RoleResponse>> GetRoleAsync([FromRoute][Required] string id)
        {
            var result = await roleService.GetRoleAsync(id);
            if (!result.Success)
            {
                return NotFound();
            }

            return Ok(new RoleResponse { Role = result.Role });
        }
    }
}