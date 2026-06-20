using Asp.Versioning;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Requests;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace IdentityServiceApi.Features.UserManagement.Controllers
{
    /// <summary>
    ///     Provides endpoints for managing user passwords, including setting and updating passwords.
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
        ///     Sets a user's password, typically used during initial account setup or reset scenarios.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user whose password is being set.
        /// </param>
        /// <param name="request">
        ///     The request containing the new password details.
        /// </param>
        /// <returns>
        ///     Returns a status indicating the outcome of the operation.
        /// </returns>
        /// <response code="204">
        ///     The password was successfully set.
        /// </response>
        /// <response code="400">
        ///     The request was invalid or failed validation.
        /// </response>
        /// <response code="404">
        ///     The specified user could not be found.
        /// </response>
        [AllowAnonymous]
        [HttpPut("users/{id}/password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
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
        ///     Updates an existing user's password.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user whose password is being updated.
        /// </param>
        /// <param name="request">
        ///     The request containing the current and new password details.
        /// </param>
        /// <returns>
        ///     Returns a status indicating the outcome of the operation.
        /// </returns>
        /// <response code="204">
        ///     The password was successfully updated.
        /// </response>
        /// <response code="400">
        ///     The request was invalid or failed validation.
        /// </response>
        /// <response code="403">
        ///     The user is not authorized to update the password.
        /// </response>
        [Authorize(Roles = RoleGroups.AllStandardRoles)]
        [HttpPatch("users/{id}/password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
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