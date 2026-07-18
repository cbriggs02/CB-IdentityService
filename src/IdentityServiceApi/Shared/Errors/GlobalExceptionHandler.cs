using IdentityServiceApi.Shared.Constants;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Shared.Models;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;

namespace IdentityServiceApi.Shared.Errors
{
    /// <summary>
    ///     Handles all unhandled exceptions that occur during the HTTP request pipeline
    ///     and converts them into a standardized JSON error response.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class GlobalExceptionHandler(IServiceScopeFactory scopeFactory) : IExceptionHandler
    {
        /// <summary>
        ///     Attempts to handle an unhandled exception and write a standardized response.
        /// </summary>
        /// <param name="httpContext">
        ///     The current HTTP context for the request.
        /// </param>
        /// <param name="exception">
        ///     The exception that was thrown during request processing.
        /// </param>
        /// <param name="cancellationToken">
        ///     A token to monitor for request cancellation.
        /// </param>
        /// <returns>
        ///     A <see cref="ValueTask{Boolean}"/> indicating whether the exception was handled.
        ///     Always returns <c>true</c> when the exception is processed.
        /// </returns>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();
            var loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();

            var logEntry = new LogEntry
            {
                LogLevel = LogLevel.Error,
                LogSource = LogSource.GlobalExceptionMiddleware,
                Message = "An unhandled exception occurred",
                Exception = exception
            };

            loggerService.LogData(logEntry);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                Errors = [ErrorMessages.General.GlobalExceptionMessage]
            };

            var jsonResponse = JsonConvert.SerializeObject(response);
            await httpContext.Response.WriteAsync(jsonResponse, cancellationToken);

            return true;
        }
    }
}