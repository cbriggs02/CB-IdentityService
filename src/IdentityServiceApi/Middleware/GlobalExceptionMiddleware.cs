using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Shared.Models;
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
    ///     @Updated: 2026
    /// </remarks>
    public class GlobalExceptionMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        private readonly RequestDelegate _next = next;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

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
                LogExceptionDetails(loggerService, ex);
                await WriteServerErrorResponseAsync(context);
            }
        }

        private static void LogExceptionDetails(ILoggerService loggerService, Exception ex)
        {
            var logEntry = new LogEntry
            {
                LogLevel = LogLevel.Error,
                LogSource = LogSource.GlobalExceptionMiddleware,
                Message = "An unhandled exception occurred",
                Exception = ex
            };

            loggerService.LogData(logEntry);
        }

        private static async Task WriteServerErrorResponseAsync(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                Errors = [ErrorMessages.General.GlobalExceptionMessage]
            };

            var jsonResponse = JsonConvert.SerializeObject(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
