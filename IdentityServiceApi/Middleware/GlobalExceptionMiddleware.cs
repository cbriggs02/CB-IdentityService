﻿using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Logging;
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
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
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
                ConsoleLogExceptionDetails(context, ex);
                await WriteServerErrorResponseAsync(context);
            }
        }

        private void ConsoleLogExceptionDetails(HttpContext context, Exception ex)
        {
            var exceptionMessage = ex.Message ?? "No exception message";
            var requestPath = context.Request.Path.ToString() ?? "No request path";
            var timestamp = DateTime.UtcNow;

            _logger.LogError(ex, "{Message}. Exception occurred at {Timestamp}. Request: {Path}, Exception Message: {ExceptionMessage}",
                "An unhandled exception occurred", timestamp, requestPath, exceptionMessage);
        }

        private static async Task WriteServerErrorResponseAsync(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = ErrorMessages.General.GlobalExceptionMessage
            };

            var jsonResponse = JsonConvert.SerializeObject(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
