using IdentityServiceApi.Models.ApiResponseModels.LoginResponses;
using IdentityServiceApi.Models.ApiResponseModels.Shared;
using IdentityServiceApi.Models.RequestModels.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using IdentityServiceApi.Controllers;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace IdentityServiceApi.Tests.Integration.Controllers
{
    /// <summary>
    ///     Unit tests for the <see cref="LoginController"/> class.
    ///     This class contains test cases for various login controller HTTP/HTTPS scenarios, verifying the 
    ///     behavior of the login controller functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "IntegrationTest")]
    public class LoginControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private const string RequestUri = "/api/v1/Login/tokens";

        /// <summary>
        ///     Initializes a new instance of the <see cref="LoginControllerTests"/> class.
        ///     This constructor sets up the HTTP client for testing using the provided <see cref="WebApplicationFactory{TEntryPoint}"/>.
        /// </summary>
        /// <param name="factory">
        ///     The web application factory used to create the client for integration testing.
        /// </param>
        public LoginControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        /// <summary>
        ///     Verifies that the <see cref="LoginController.Login"/> method returns a Not Found response (404) 
        ///     when the user does not exist in the system.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task Login_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var jsonContent = SetupJsonPayloadContent("non existent user", "password");

            // Act
            var response = await _client.PostAsync(RequestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        ///     Verifies that the <see cref="LoginController.Login"/> method returns a Bad Request response (400) 
        ///     when the provided credentials are invalid.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task Login_ReturnsBadRequest_WhenCredentialsAreInvalid()
        {
            // Arrange
            var jsonContent = SetupJsonPayloadContent("admin@admin.com", "wrong password");

            // Act
            var response = await _client.PostAsync(RequestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

            Assert.NotNull(errorResponse);
            Assert.NotEmpty(errorResponse.Errors);
        }

        /// <summary>
        ///     Verifies that the <see cref="LoginController.Login"/> method returns an OK response (200) 
        ///     when the provided credentials are valid.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task Login_ReturnsOK_WhenCredentialsAreValid()
        {
            // Arrange
            var jsonContent = SetupJsonPayloadContent("admin@admin.com", "AdminPassword123!");

            // Act
            var response = await _client.PostAsync(RequestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseBody);

            Assert.NotNull(loginResponse);
            Assert.NotEmpty(loginResponse.Token);
        }

        private static StringContent SetupJsonPayloadContent(string username, string password)
        {
            var credentials = new LoginRequest
            {
                UserName = username,
                Password = password,
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(credentials),
                Encoding.UTF8,
                "application/json"
            );
            return jsonContent;
        }
    }
}
