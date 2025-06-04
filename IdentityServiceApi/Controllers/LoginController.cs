using Asp.Versioning;
using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Authentication;
using IdentityServiceApi.Models.ApiResponseModels.Shared;
using IdentityServiceApi.Models.ApiResponseModels.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using IdentityServiceApi.Models.RequestModels.Authentication;

namespace IdentityServiceApi.Controllers
{
    /// <summary>
    ///     Controller for handling API operations related to user authentication.
    ///     This controller processes all incoming requests associated with login management 
    ///     and delegates these requests to the login service, which encapsulates the business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/login")]
    [AllowAnonymous]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LoginController"/> class with the specified dependencies.
        /// </summary>
        /// <param name="loginService">
        ///     Login service used for all login-related operations.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public LoginController(ILoginService loginService)
        {
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
        }

        /// <summary>
        ///     Asynchronously processes all requests for logging in a user using their credentials.
        ///     This method delegates the login process to the required service.
        /// </summary>
        /// <param name="credentials">
        ///     The <see cref="LoginRequest"/> model containing the user's login credentials.
        /// </param>
        /// <returns>
        ///     Returns an action result:
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) with a JWT token if the login is successful.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors 
        ///         returned by the login service that occurred during the login attempt.
        ///          - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the credentials are 
        ///          invalid or the account is not activated.
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        ///     - <see cref="StatusCodes.Status500InternalServerError"/> (Internal Server Error) if an 
        ///     unexpected error occurs during the login process.
        /// </returns>
        [HttpPost("tokens")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
