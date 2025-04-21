using Microsoft.AspNetCore.Mvc.Testing;
using IdentityServiceApi.Middleware;
using Bogus;
using IdentityServiceApi.Data;
using IdentityServiceApi.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Constants;
using System.Text;
using IdentityServiceApi.Models.RequestModels.Authentication;
using IdentityServiceApi.Tests.Integration.Constants;
using IdentityServiceApi.Models.ApiResponseModels.Login;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;

namespace IdentityServiceApi.Tests.Integration.Middleware
{
    /// <summary>
    ///     Integration tests for the <see cref="TokenValidatorMiddleware"/> middleware.
    ///     This test class ensures that the token validation logic correctly handles scenarios
    ///     such as invalid or deleted users.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "IntegrationTest")]
    public class TokenValidatorMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private const string Password = "Test@1234";

        /// <summary>
        ///     Initializes a new instance of the <see cref="TokenValidatorMiddleware"/> class.
        ///     This constructor sets up the test environment using a <see cref="WebApplicationFactory{TEntryPoint}"/> 
        ///     to create a test server for the application.
        /// </summary>
        /// <param name="factory">
        ///     The <see cref="WebApplicationFactory{Program}"/> instance used to create the test server.
        /// </param>
        public TokenValidatorMiddlewareTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        /// <summary>
        ///     Tests that the <see cref="TokenValidatorMiddleware"/> correctly returns a 401 Unauthorized
        ///     response when the user associated with the token is deleted.
        /// </summary>
        /// <returns>
        ///     A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task TokenValidator_ShouldReturn401_WhenUserIsDeleted()
        {
            var client = _factory.CreateClient();
            var user = await CreateTestUserAsync(true);

            var jsonContent = SetupJsonPayloadContent(user.UserName, Password);

            // Act : login and obtain token
            var loginResponse = await client.PostAsync(ApiRoutes.LoginController.RequestUri, jsonContent);
            var token = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

            // Act : delete user who token belongs too 
            await client.DeleteAsync($"{ApiRoutes.UsersController.BaseUri}/{user.Id}");

            // Act : make request to restricted resource
            var securedResponse = await client.GetAsync($"{ApiRoutes.TestController.BaseUri}/simulateRestrictedRequest");

            Assert.Equal(HttpStatusCode.Unauthorized, securedResponse.StatusCode);
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
            var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var applicationDbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var createTestUserHelper = new CreateTestUserHelper(userManager, applicationDbContext);

            var faker = new Faker();
            string userName = faker.Internet.UserName();
            string email = faker.Internet.Email();
            string firstName = faker.Name.FirstName();
            string lastName = faker.Name.LastName();
            string phoneNumber = faker.Phone.PhoneNumber();

            return await createTestUserHelper.CreateTestUserWithPasswordAsync(userName, firstName, lastName, email, phoneNumber, Password, status, Roles.User);
        }
    }
}
