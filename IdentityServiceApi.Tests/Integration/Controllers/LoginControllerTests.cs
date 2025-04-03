using IdentityServiceApi.Models.ApiResponseModels.LoginResponses;
using IdentityServiceApi.Models.ApiResponseModels.Shared;
using IdentityServiceApi.Models.RequestModels.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using IdentityServiceApi.Controllers;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Bogus;
using IdentityServiceApi.Tests.Integration.Constants;
using IdentityServiceApi.Data;

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
        private readonly WebApplicationFactory<Program> _factory;
        private const string Password = "Test@1234";

        /// <summary>
        ///     Initializes a new instance of the <see cref="LoginControllerTests"/> class.
        ///     This constructor sets up the test environment using a <see cref="WebApplicationFactory{TEntryPoint}"/> 
        ///     to create a test server for the application.
        /// </summary>
        /// <param name="factory">
        ///     The <see cref="WebApplicationFactory{Program}"/> instance used to create the test server.
        /// </param>
        public LoginControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        /// <summary>
        ///     Verifies that the <see cref="LoginController.LoginAsync"/> method returns a Not Found response (404) 
        ///     when the user does not exist in the system.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task LoginAsync_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var client = _factory.CreateClient();
            var jsonContent = SetupJsonPayloadContent("non existent user", "password");

            // Act
            var response = await client.PostAsync(ApiRoutes.LoginController.RequestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        ///     Verifies that the <see cref="LoginController.LoginAsync"/> method returns a Bad Request response (400) 
        ///     when the provided credentials are invalid.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task LoginAsync_ReturnsBadRequest_WhenCredentialsAreInvalid()
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = await CreateTestUserAsync(true);
            var jsonContent = SetupJsonPayloadContent(user.UserName, "wrong password");

            // Act
            var response = await client.PostAsync(ApiRoutes.LoginController.RequestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

            Assert.NotNull(errorResponse);
            Assert.NotEmpty(errorResponse.Errors);

            await CleanUpTestUserAsync(user.Email);
        }

        /// <summary>
        ///     Verifies that the <see cref="LoginController.LoginAsync"/> method returns Bad Request (400)
        ///     when the user's account is not activated.
        /// </summary>
        /// <returns>
        ///      A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task LoginAsync_ReturnsBadRequest_WhenUsersNotActivated()
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = await CreateTestUserAsync(false);
            var jsonContent = SetupJsonPayloadContent(user.UserName, Password);

            // Act
            var response = await client.PostAsync(ApiRoutes.LoginController.RequestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

            Assert.NotNull(errorResponse);
            Assert.NotEmpty(errorResponse.Errors);

            await CleanUpTestUserAsync(user.Email);
        }

        /// <summary>
        ///     Verifies that the <see cref="LoginController.LoginAsync"/> method returns an OK response (200) 
        ///     when the provided credentials are valid.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task LoginAsync_ReturnsOK_WhenCredentialsAreValid()
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = await CreateTestUserAsync(true);
            var jsonContent = SetupJsonPayloadContent(user.UserName, Password);

            // Act
            var response = await client.PostAsync(ApiRoutes.LoginController.RequestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseBody);

            Assert.NotNull(loginResponse);
            Assert.NotEmpty(loginResponse.Token);

            await CleanUpTestUserAsync(user.Email);
        }

        private static StringContent SetupJsonPayloadContent(string username, string password)
        {
            var credentials = new LoginRequest
            {
                UserName = username,
                Password = password,
            };

            var jsonContent = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(credentials),
                Encoding.UTF8,
                "application/json"
             );
            return jsonContent;
        }

        private async Task<User> CreateTestUserAsync(bool status)
        {
            var createTestUserHelper = CreateTestUserHelper();

            var faker = new Faker();
            string userName = faker.Internet.UserName();
            string email = faker.Internet.Email();
            string firstName = faker.Name.FirstName();
            string lastName = faker.Name.LastName();
            string phoneNumber = faker.Phone.PhoneNumber();
            string country = faker.Address.Country();

            return await createTestUserHelper.CreateTestUserWithPasswordAsync(userName, firstName, lastName, email, phoneNumber, country, Password, status);
        }

        private async Task CleanUpTestUserAsync(string email)
        {
            var createTestUserHelper = CreateTestUserHelper();
            await createTestUserHelper.DeleteTestUserAsync(email);
        }

        private CreateTestUserHelper CreateTestUserHelper()
        {
            var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var applicationDbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var createTestUserHelper = new CreateTestUserHelper(userManager, applicationDbContext);
            return createTestUserHelper;
        }
    }
}
