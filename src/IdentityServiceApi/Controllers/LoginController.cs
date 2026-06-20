using Asp.Versioning;
using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Authentication;
using IdentityServiceApi.Models.ApiResponseModels.Login;
using IdentityServiceApi.Models.ApiResponseModels.Shared;
using IdentityServiceApi.Models.RequestModels.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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
    [Route("api/v{version:apiVersion}/login")]
    [AllowAnonymous]
    public class LoginController(ILoginService loginService) : ControllerBase
    {
        private readonly ILoginService _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        [HttpPost("tokens")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.LoginApi.Login)]
        public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest credentials)
        {
            var result = await _loginService.LoginAsync(credentials);
            if (!result.Success)
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                return BadRequest(new ErrorResponse { Errors = result.Errors });
            }

            if (string.IsNullOrEmpty(result.Token))
            {
                return Unauthorized();
            }

            return Ok(new LoginResponse { Token = result.Token });
        }
    }
}
