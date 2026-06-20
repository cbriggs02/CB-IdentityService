using Asp.Versioning;
using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Authorization;
using IdentityServiceApi.Models.ApiResponseModels.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [Route("api/v{version:apiVersion}/roles")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class RolesController(IRoleService roleService) : ControllerBase
    {
        private readonly IRoleService _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RolesListResponse))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
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
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.RolesApi.GetRoleById)]
        public async Task<ActionResult<RoleResponse>> GetRoleAsync([FromRoute][Required] string id)
        {
            var result = await _roleService.GetRoleAsync(id);
            if (!result.Success && result.Errors.Any(error => error.Contains(ErrorMessages.Role.NotFound, StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound();
            }

            return Ok(new RoleResponse { Role = result.Role });
        }
    }
}
