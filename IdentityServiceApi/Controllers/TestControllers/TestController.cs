using IdentityServiceApi.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

#if DEBUG
namespace IdentityServiceApi.Controllers.TestControllers
{
    /// <summary>
    ///     A test controller used to simulate integration testing purposes.
    ///     This controller is not to be included in API document or in production builds.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025  
    /// </remarks>
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        /// <summary>
        ///     Throws an <see cref="InvalidOperationException"/> to simulate an error scenario in integration tests.
        ///     This endpoint is used to verify the exception handling and logging functionality of t
        ///     he application's middleware.
        /// </summary>
        /// <returns>
        ///     A <see cref="Task"/> representing the asynchronous operation, which will never complete 
        ///     successfully as an exception is thrown.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Always thrown when this endpoint is accessed. This exception is used to test the 
        ///     exception handling middleware.
        /// </exception>
        [AllowAnonymous]
        [HttpGet("throwException")]
        public IActionResult ThrowException()
        {
            throw new InvalidOperationException("This is a test exception.");
        }

        /// <summary>
        ///     Simulates a slow-performing endpoint by delaying the response.
        ///     Used to trigger performance logging in middleware.
        /// </summary>
        /// <returns>
        ///     A <see cref="Task{TResult}"/> representing the asynchronous operation that returns 
        ///     an <see cref="IActionResult"/> containing a success message upon completion of the delay.
        /// </returns>
        [AllowAnonymous]
        [HttpGet("simulateSlowRequest")]
        public async Task<IActionResult> SimulateSlowRequest()
        {
            await Task.Delay(1500);
            return Ok("Slow response complete.");
        }

        /// <summary>
        ///     Simulates a normal-performing endpoint with minimal delay.
        ///     Used to verify that performance logging only occurs for slow requests.
        /// </summary>
        /// <returns>
        ///     A <see cref="Task{TResult}"/> representing the asynchronous operation that returns 
        ///     an <see cref="IActionResult"/> with a success message after a short delay.
        /// </returns>
        [AllowAnonymous]
        [HttpGet("simulateNormalRequest")]
        public async Task<IActionResult> SimulateNormalRequest()
        {
            await Task.Delay(200);
            return Ok("Normal response complete.");
        }

        /// <summary>
        ///     Simulates a request to a protected resource that requires authentication.
        /// </summary>
        /// <returns>
        ///     A 204 No Content response if the user is authorized; otherwise, a 401 Unauthorized
        ///     response is returned by the middleware before reaching this endpoint.
        /// </returns>
        [Authorize(Roles = Roles.User)]
        [HttpGet("simulateRestrictedRequest")]
        public IActionResult SimulateRestrictedRequest()
        {
            return NoContent();
        }
    }
}
#endif
