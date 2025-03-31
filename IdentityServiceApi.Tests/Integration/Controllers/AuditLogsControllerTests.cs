using IdentityServiceApi.Controllers;
using IdentityServiceApi.Models.ApiResponseModels.AuditLogsResponses;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using IdentityServiceApi.Tests.Integration.Helpers;
using IdentityServiceApi.Constants;

namespace IdentityServiceApi.Tests.Integration.Controllers
{
    /// <summary>
    ///     Unit tests for the <see cref="AuditLogsController"/> class.
    ///     This class contains test cases for various audit logs controller HTTP/HTTPS scenarios, verifying the 
    ///     behavior of the audit logs controller functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "IntegrationTest")]
    public class AuditLogsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuditLogsControllerTests"/> class.
        ///     This constructor sets up the HTTP client for testing using the provided <see cref="WebApplicationFactory{TEntryPoint}"/>.
        /// </summary>
        /// <param name="factory">
        ///     The web application factory used to create the client for integration testing.
        /// </param>
        public AuditLogsControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        /// <summary>
        ///     Verifies that the <see cref="AuditLogsController.GetLogs"/> method returns an OK response (200) 
        ///     when the user is authorized and audit logs data exists.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task GetLogs_ReturnsOk_WhenAuthorizedAndDataExists()
        {
            // Arrange
            AuthenticateClient();
            var requestUri = "/api/v1/AuditLogs?Page=1&PageSize=5&Action=0";

            // Act
            var response = await _client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AuditLogListResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(result);
            Assert.NotEmpty(result.Logs);
        }

        /// <summary>
        ///     Verifies that the <see cref="AuditLogsController.GetLogs"/> method returns an Unauthorized response (401) 
        ///     when the user is not authenticated.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task GetLogs_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            // Arrange
            var requestUri = "/api/v1/AuditLogs?PageNumber=1&PageSize=5&AuditLogs?Page=1&PageSize=5&Action=0";

            // Act
            var response = await _client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        ///     Verifies that the <see cref="AuditLogsController.DeleteLog"/> method returns a NotFound response (404) 
        ///     when trying to delete a log that does not exist.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task DeleteLog_ReturnsNotFound_WhenLogDoesNotExist()
        {
            // Arrange
            AuthenticateClient();
            var requestUri = "/api/v1/AuditLogs/nonexistent-log-id";

            // Act
            var response = await _client.DeleteAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        ///     Verifies that the <see cref="AuditLogsController.DeleteLog"/> method returns an Unauthorized response (401) 
        ///     when the user is not authenticated.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task DeleteLog_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            // Arrange
            var requestUri = "/api/v1/AuditLogs/valid-log-id";

            // Act
            var response = await _client.DeleteAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private void AuthenticateClient()
        {
            var token = JwtTokenTestHelper.GenerateJwtToken(roles: new List<string> { Roles.SuperAdmin });
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
