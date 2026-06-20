using Asp.Versioning;
using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Models.ApiResponseModels.Shared;
using IdentityServiceApi.Models.RequestModels.UserManagement;
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
    [Route("api/v{version:apiVersion}/password")]
    public class PasswordController(IPasswordService passwordService) : ControllerBase
    {
        private readonly IPasswordService _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPut("users/{id}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.PasswordApi.SetPassword)]
        public async Task<IActionResult> SetPasswordAsync([FromRoute][Required] string id, [FromBody] SetPasswordRequest request)
        {
            var result = await _passwordService.SetPasswordAsync(id, request);
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
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = RoleGroups.AllStandardRoles)]
        [HttpPatch("users/{id}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(Summary = ApiDocumentation.PasswordApi.UpdatePassword)]
        public async Task<IActionResult> UpdatePasswordAsync([FromRoute][Required] string id, [FromBody] UpdatePasswordRequest request)
        {
            var result = await _passwordService.UpdatePasswordAsync(id, request);
            if (!result.Success)
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.Authorization.Forbidden, StringComparison.OrdinalIgnoreCase)))
                {
                    return Forbid();
                }

                return BadRequest(new ErrorResponse { Errors = result.Errors });
            }

            return NoContent();
        }
    }
}
