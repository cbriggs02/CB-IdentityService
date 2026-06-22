using Asp.Versioning;
using IdentityServiceApi.Features.Authentication.Interfaces;
using IdentityServiceApi.Features.Authentication.Models;
using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Models;
using IdentityServiceApi.Shared.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IdentityServiceApi.Features.Authentication.Controllers
{
    /// <summary>
    ///     Provides endpoints for user authentication and token generation.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2024  
    ///     @Updated: 2026  
    /// </remarks>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/login")]
    [AllowAnonymous]
    public class LoginController(ILoginService loginService) : ControllerBase
    {
        /// <summary>
        ///     Authenticates a user and generates a JWT token upon successful login.
        /// </summary>
        /// <param name="credentials">
        ///     The login request containing the user's authentication credentials.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="LoginResponse"/> containing a JWT token if authentication is successful.
        /// </returns>
        /// <response code="200">
        ///     Authentication succeeded and a JWT token is returned.
        /// </response>
        /// <response code="400">
        ///     The request is invalid or authentication failed due to validation errors.
        /// </response>
        /// <response code="401">
        ///     Authentication failed due to invalid credentials or missing token generation.
        /// </response>
        /// <response code="404">
        ///     The specified user could not be found.
        /// </response>
        [HttpPost("tokens")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.LoginApi.Login)]
        public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest credentials)
        {
            var result = await loginService.LoginAsync(credentials);
            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ErrorType.Unauthorized => Unauthorized(new ErrorResponse
                    {
                        Errors = result.Errors
                    }),
                    ErrorType.InvalidState => Unauthorized(new ErrorResponse
                    {
                        Errors = result.Errors
                    }),
                    ErrorType.NotFound => Unauthorized(),
                    _ => Unauthorized()
                };
            }
            return Ok(new LoginResponse { Token = result.Token! });
        }
    }
}