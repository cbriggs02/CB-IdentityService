﻿using AspNetWebService.Constants;
using AspNetWebService.Interfaces.UserManagement;
using AspNetWebService.Models.ApiResponseModels;
using AspNetWebService.Models.RequestModels.PasswordRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace AspNetWebService.Controllers
{
    /// <summary>
    ///     Controller for handling API operations related to passwords.
    ///     This controller processes all incoming requests related to password management and delegates
    ///     them to the password service, which implements the business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordApiController : ControllerBase
    {
        private readonly IPasswordService _passwordService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PasswordApiController"/> class with the specified dependencies.
        /// </summary>
        /// <param name="passwordService">
        ///     Password service used for all password-related operations.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public PasswordApiController(IPasswordService passwordService)
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
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if setting the password was successful.
        ///     
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors 
        ///         returned by the password service that occurred while setting the password.
        ///       
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        /// </returns>
        [AllowAnonymous]
        [HttpPut("set/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.PasswordApi.SetPassword)]
        public async Task<IActionResult> SetPassword([FromRoute][Required] string id, [FromBody] SetPasswordRequest request)
        {
            var result = await _passwordService.SetPassword(id, request);

            if (result.Success)
            {
                return Ok();
            }
            else
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                return BadRequest(new ErrorApiResponse { Errors = result.Errors });
            }
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
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if updating the password was successful.
        ///     
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors 
        ///         returned by the password service that occurred during the password update.
        ///       
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by
        ///         a user who is not authenticated or does not have the required role.
        ///     
        ///     - <see cref="StatusCodes.Status403Forbidden"/> (Forbidden) if an authorized user attempts 
        ///         to update another user's password or a admin attempts to update another admins password.
        ///       
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        /// </returns>
        [Authorize(Roles = "SuperAdmin,Admin,User")]
        [HttpPut("update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.PasswordApi.UpdatePassword)]
        public async Task<IActionResult> UpdatePassword([FromRoute][Required] string id, [FromBody] UpdatePasswordRequest request)
        {
            var result = await _passwordService.UpdatePassword(id, request);

            if (result.Success)
            {
                return Ok();
            }
            else
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.Authorization.Forbidden, StringComparison.OrdinalIgnoreCase)))
                {
                    return Forbid();
                }

                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                return BadRequest(new ErrorApiResponse { Errors = result.Errors });
            }
        }
    }
}
