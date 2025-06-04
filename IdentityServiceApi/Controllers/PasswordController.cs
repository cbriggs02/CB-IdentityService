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
    ///     Controller for handling API operations related to passwords.
    ///     This controller processes all incoming requests related to password management and delegates
    ///     them to the password service, which implements the business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/password")]
    public class PasswordController : ControllerBase
    {
        private readonly IPasswordService _passwordService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PasswordController"/> class with the specified dependencies.
        /// </summary>
        /// <param name="passwordService">
        ///     Password service used for all password-related operations.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public PasswordController(IPasswordService passwordService)
        {
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        }

        /// <summary>
        ///     Asynchronously processes requests for setting a user's password in the system 
        ///     by delegating the operation to the required service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user whose password will be updated.
        /// </param>
        /// <param name="request">
        ///     A model object containing the password details, including the new password and 
        ///     its confirmation.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status204NoContent"/> (NoContent) if setting the password was successful.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors 
        ///         returned by the password service that occurred while setting the password.       
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        ///     - <see cref="StatusCodes.Status500InternalServerError"/> (Internal Server Error) if an unexpected error occurs. 
        /// </returns>
        [AllowAnonymous]
        [HttpPut("users/{id}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        ///     Asynchronously processes requests for updating a user's password in the system 
        ///     by delegating the operation to the required service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user whose password will be updated.
        /// </param>
        /// <param name="request">
        ///     A model object containing the necessary details for updating the password, 
        ///     including the current password and the new password.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status204NoContent"/> (NoContent) if updating the password was successful.   
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors 
        ///         returned by the password service that occurred during the password update.      
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by
        ///         a user who is not authenticated or does not have the required role.
        ///     - <see cref="StatusCodes.Status403Forbidden"/> (Forbidden) if an authorized user attempts 
        ///         to update another user's password or a admin attempts to update another admins password.      
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        ///     - <see cref="StatusCodes.Status500InternalServerError"/> (Internal Server Error) if an unexpected error occurs. 
        /// </returns>
        [Authorize(Roles = RoleGroups.AllStandardRoles)]
        [HttpPatch("users/{id}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
