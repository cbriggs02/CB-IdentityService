using Microsoft.AspNetCore.Mvc.Testing;
using IdentityServiceApi.Middleware;
using IdentityServiceApi.Controllers.TestControllers;
using Newtonsoft.Json;
using IdentityServiceApi.Models.ApiResponseModels.Shared;
using System.Net;
using IdentityServiceApi.Constants;
using IdentityServiceApi.Tests.Integration.Constants;

namespace IdentityServiceApi.Tests.Integration.Middleware
{
    /// <summary>
    ///     Contains integration tests for the <see cref="GlobalExceptionMiddleware"/> to verify that exceptions 
    ///     are properly logged and that the correct HTTP response status is returned when an exception is thrown.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "IntegrationTest")]
    public class GlobalExceptionMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        /// <summary>
		///     Initializes a new instance of the <see cref="GlobalExceptionMiddleware"/> class.
		///     This constructor sets up the test environment using a <see cref="WebApplicationFactory{TEntryPoint}"/> 
		///     to create a test server for the application.
		/// </summary>
		/// <param name="factory">
		///     The <see cref="WebApplicationFactory{Program}"/> instance used to create the test server.
		/// </param>
        public GlobalExceptionMiddlewareTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        /// <summary>
        ///     Verifies that the <see cref="GlobalExceptionMiddleware"/> correctly handles exceptions and logs the error,
        ///     returning an Internal Server Error (500) response when an exception is thrown in the
        ///     <see cref="TestController.ThrowException"/> endpoint.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation, verifying that an exception is logged and an 
        ///     Internal Server Error (500) response with the correct error message is returned.
        /// </returns>
        [Fact]
        public async Task Should_Log_Exception_And_Return_InternalServerError_When_Exception_IsThrown()
        {
            // Arrange
            var client = _factory.CreateClient();
            string requestUri = $"{ApiRoutes.TestController.BaseUri}/throwException";

            // Act
            var response = await client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

            Assert.NotNull(errorResponse);
            Assert.Single(errorResponse.Errors);
            Assert.Equal(ErrorMessages.General.GlobalExceptionMessage, errorResponse.Errors.First());
        }
    }
}
