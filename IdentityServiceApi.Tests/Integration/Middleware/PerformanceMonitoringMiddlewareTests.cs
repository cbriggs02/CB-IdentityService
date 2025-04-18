using IdentityServiceApi.Middleware;
using IdentityServiceApi.Tests.Integration.Constants;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IdentityServiceApi.Tests.Integration.Middleware
{
    /// <summary>
    ///     Contains integration tests for the <see cref="PerformanceMonitoringMiddleware"/> to verify that http requests
    ///     metrics are being recorded and when a request takes longer then expected according to system standard 
    ///     it is logged.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "IntegrationTest")]
    public class PerformanceMonitoringMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        /// <summary>
		///     Initializes a new instance of the <see cref="PerformanceMonitoringMiddleware"/> class.
		///     This constructor sets up the test environment using a <see cref="WebApplicationFactory{TEntryPoint}"/> 
		///     to create a test server for the application.
		/// </summary>
		/// <param name="factory">
		///     The <see cref="WebApplicationFactory{Program}"/> instance used to create the test server.
		/// </param>
        public PerformanceMonitoringMiddlewareTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        /// <summary>
        ///     Verifies that the <see cref="PerformanceMonitoringMiddleware"/> allows requests exceeding
        ///     the expected threshold to complete successfully. This test calls the <c>simulateSlowRequest</c>
        ///     endpoint, which intentionally delays its response to simulate slow performance.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous test operation, ensuring that the middleware does not interfere with 
        ///     slow requests and still returns a successful response.
        /// </returns>
        [Fact]
        public async Task Middleware_LogsPerformance_WhenRequestIsSlow()
        {
            // Arrange
            var client = _factory.CreateClient();
            var requestUri = $"{ApiRoutes.TestController.BaseUri}/simulateSlowRequest";

            // Act
            var response = await client.GetAsync(requestUri);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        ///     Verifies that the <see cref="PerformanceMonitoringMiddleware"/> allows fast requests to complete 
        ///     successfully without interference. This test calls the <c>simulateNormalRequest</c> endpoint,
        ///     which completes quickly and simulates normal system performance.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous test operation, verifying the middleware handles normal requests
        ///     without blocking or failing.
        /// </returns>
        [Fact]
        public async Task Middleware_LogsPerformance_WhenRequestIsNormal()
        {
            // Arrange
            var client = _factory.CreateClient();
            var requestUri = $"{ApiRoutes.TestController.BaseUri}/simulateNormalRequest";

            // Act
            var response = await client.GetAsync(requestUri);

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }
}
