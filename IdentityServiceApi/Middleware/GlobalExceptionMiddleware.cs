using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Logging;
using IdentityServiceApi.Models.ApiResponseModels.Shared;
using Newtonsoft.Json;

namespace IdentityServiceApi.Middleware
{
    /// <summary>
    ///     Middleware for handling exceptions globally and providing a standardized error response.
    ///     Captures and logs detailed exception information while ensuring a consistent JSON error response for clients.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IWebHostEnvironment _env;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GlobalExceptionMiddleware"/> class.
        /// </summary>
        /// <param name="next">
        ///     The delegate representing the next middleware in the pipeline.
        /// </param>
        /// <param name="logger">
        ///     The logger instance for logging exceptions.
        /// </param>
        /// <param name="scopeFactory">
        ///     The factory for creating service scopes to resolve scoped services.
        /// </param>
        /// <param name="env">
        ///     The environment in which the application is running. This parameter is an instance of 
        ///     <see cref="IWebHostEnvironment"/> and provides information about the
        ///     application's environment (e.g., Development, Staging, Production). It is used 
        ///     to configure environment-specific behaviors, such as logging or error handling, 
        ///     based on the current environment.
        /// </param>
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IServiceScopeFactory scopeFactory, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _env = env;
        }

        /// <summary>
        ///     Asynchronously invokes the exception handling middleware. If an exception occurs during 
        ///     the HTTP request processing, it is caught, logged, and a standardized JSON error response 
        ///     is returned to the client.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="HttpContext"/> representing the current HTTP request, providing access to 
        ///     request and response data.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of processing the request and handling any exceptions.
        /// </returns>
        public async Task InvokeAsync(HttpContext context)
        {
            using var scope = _scopeFactory.CreateScope();
            var loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await loggerService.LogExceptionAsync(ex);
                LogExceptionDetails(context, ex);
                await WriteServerErrorResponseAsync(context);
            }
        }

        private void LogExceptionDetails(HttpContext context, Exception ex)
        {
            var requestPath = context.Request.Path.ToString() ?? "No request path";
            var timestamp = DateTime.UtcNow;

            if (_env.IsProduction())
            {
                _logger.LogError("Unhandled exception at {Timestamp}. Path: {Path}", timestamp, requestPath);
            }
            else
            {
                _logger.LogError(ex,
                    "Unhandled exception at {Timestamp}. Path: {Path}. Message: {Message}. Exception Type: {ExceptionType}. Stack Trace: {StackTrace}. Inner Exception: {InnerException}",
                    timestamp, requestPath, ex.Message, ex.GetType().ToString(), ex.StackTrace, ex.InnerException?.ToString() ?? "No inner exception");
            }
        }

        private static async Task WriteServerErrorResponseAsync(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                Errors = new List<string> { ErrorMessages.General.GlobalExceptionMessage }
            };

            var jsonResponse = JsonConvert.SerializeObject(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
