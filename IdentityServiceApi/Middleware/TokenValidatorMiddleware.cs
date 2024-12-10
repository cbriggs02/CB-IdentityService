﻿using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Logging;
using IdentityServiceApi.Interfaces.UserManagement;
using Newtonsoft.Json;
using System.Security.Claims;

namespace IdentityServiceApi.Middleware
{
    /// <summary>
    ///     Middleware for validating JWT tokens to ensure that tokens belonging to recently deleted users are unauthorized.
    ///     This middleware intercepts incoming requests, checks the validity of the JWT token, and verifies that the user still exists in the system.
    ///     If the user has been deleted, it marks the token as unauthorized.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class TokenValidatorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenValidatorMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TokenValidatorMiddleware"/> class.
        /// </summary>
        /// <param name="next">
        ///     The delegate representing the next middleware in the pipeline.
        /// </param>
        /// <param name="logger">
        ///     An instance of <see cref="ILogger{TokenValidator}"/> for logging token validation data.
        /// </param>
        /// <param name="scopeFactory">
        ///     The factory for creating service scopes to resolve scoped services.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public TokenValidatorMiddleware(RequestDelegate next, ILogger<TokenValidatorMiddleware> logger, IServiceScopeFactory scopeFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        /// <summary>
        ///     Asynchronously validates if the JWT token in the request belongs to a user who still exists in the system.
        ///     If the user no longer exists, the request is marked as unauthorized, and an appropriate response is returned.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="HttpContext"/> for the current request, containing authentication data.
        /// </param>
        /// </param>
        /// <returns>
        ///     A task that represents the completion of the JWT token validation and request processing.
        /// </returns>
        public async Task Invoke(HttpContext context)
        {
            using var scope = _scopeFactory.CreateScope();
            var loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();

            if (context.User.Identity.IsAuthenticated)
            {
                var userId = GetUserIdFromClaims(context.User);

                if (userId == null)
                {
                    string reason = ErrorMessages.Authorization.MissingUserIdClaim;
                    await HandleAuthorizationBreach(context, loggerService, reason, "Anonymous");
                    return;
                }

                var userLookupService = scope.ServiceProvider.GetRequiredService<IUserLookupService>();
                var userLookupResult = await userLookupService.FindUserById(userId);

                // Validate tokens that are still active/valid but user has recently removed account
                if (!userLookupResult.Success)
                {
                    string reason = $"User with ID {userId} no longer exists in the system.";
                    await HandleAuthorizationBreach(context, loggerService, reason, userLookupResult.UserFound.Id);
                    return;
                }
            }
            await _next(context);
        }

        /// <summary>
        ///     Extracts the user ID from the claims contained in the <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="principal">
        ///     The <see cref="ClaimsPrincipal"/> representing the authenticated user.
        /// </param>
        /// <returns>
        /// <returns>
        ///     The user ID as a string if found in the claims; otherwise, null.
        /// </returns>
        private static string GetUserIdFromClaims(ClaimsPrincipal principal)
        {
            var identity = principal.Identity as ClaimsIdentity;
            var userClaim = identity?.FindFirst(ClaimTypes.NameIdentifier);
            return userClaim?.Value;
        }

        /// <summary>
        ///     Handles an authorization breach by logging the incident and responding with a 401 Unauthorized status.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="HttpContext"/> for the current request.
        /// </param>
        /// <param name="loggerService">
        ///     The logger service for recording the breach.
        /// </param>
        /// <param name="reason">
        ///     The reason for the authorization breach.
        /// </param>
        /// <param name="userId">
        ///     The ID of the user involved in the breach.
        /// </param>
        private async Task HandleAuthorizationBreach(HttpContext context, ILoggerService loggerService, string reason, string userId)
        {
            var correlationId = Guid.NewGuid().ToString();
            await loggerService.LogAuthorizationBreach(); // Log auth breach in DB using audit logger
            ConsoleLogAuthorizationBreach(reason, userId);
            await WriteServerUnauthorizedResponse(context, correlationId); // Return 401 status to client
        }

        /// <summary>
        ///     Logs an unauthorized access attempt to the application console and the logger.
        /// </summary>
        /// <param name="reason">
        ///     The reason for the unauthorized access attempt, providing context for the breach.
        /// </param>
        /// <param name="userId">
        ///     The unique identifier of the user involved in the unauthorized access attempt.
        /// </param>
        private void ConsoleLogAuthorizationBreach(string reason, string userId)
        {
            _logger.LogWarning($"Unauthorized access attempt: Reason: {reason}, UserId: {userId}");
        }

        /// <summary>
        ///     Asynchronously logs an unauthorized access attempt and responds with a 401 Unauthorized status code.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="HttpContext"/> for the current request.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of sending the unauthorized response.
        /// </returns>
        private static async Task WriteServerUnauthorizedResponse(HttpContext context, string correlationId)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = ErrorMessages.Authorization.Unauthorized,
                correlationId
            };

            var jsonResponse = JsonConvert.SerializeObject(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}