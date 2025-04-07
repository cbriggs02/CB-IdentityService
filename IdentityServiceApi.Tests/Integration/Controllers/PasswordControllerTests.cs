using Microsoft.AspNetCore.Mvc.Testing;
using IdentityServiceApi.Controllers;
using Newtonsoft.Json;
using System.Text;
using IdentityServiceApi.Models.RequestModels.UserManagement;
using System.Net;
using IdentityServiceApi.Models.ApiResponseModels.Shared;
using IdentityServiceApi.Tests.Integration.Helpers;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using IdentityServiceApi.Models.Entities;
using Bogus;
using IdentityServiceApi.Constants;
using IdentityServiceApi.Tests.Integration.Constants;
using IdentityServiceApi.Data;

namespace IdentityServiceApi.Tests.Integration.Controllers
{
    /// <summary>
    ///     Unit tests for the <see cref="PasswordController"/> class.
    ///     This class contains test cases for various password controller HTTP/HTTPS scenarios, verifying the 
    ///     behavior of the password controller functionality.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    [Trait("TestCategory", "IntegrationTest")]
    public class PasswordControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private const string Password = "Test@1234";

        /// <summary>
        ///     Initializes a new instance of the <see cref="PasswordControllerTests"/> class.
        ///     This constructor sets up the test environment using a <see cref="WebApplicationFactory{TEntryPoint}"/> 
        ///     to create a test server for the application.
        /// </summary>
        /// <param name="factory">
        ///     The <see cref="WebApplicationFactory{Program}"/> instance used to create the test server.
        /// </param>
        public PasswordControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        /// <summary>
        ///     Verifies that the <see cref="PasswordController.SetPasswordAsync"/> method returns a NotFound response (404) 
        ///     when the specified user does not exist.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task SetPasswordAsync_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var client = _factory.CreateClient();
            const string RequestUri = ApiRoutes.PasswordController.BaseUri + "users/nonexistent-user-id/password";
            var jsonContent = SetupJsonPayloadContentSetPasswordRequest();

            // Act
            var response = await client.PutAsync(RequestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        ///     Verifies that the <see cref="PasswordController.SetPasswordAsync"/> method returns a BadRequest response (400) 
        ///     when the provided password does not match the confirmed password.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task SetPasswordAsync_ReturnsBadRequest_WhenPasswordDoesNotMatchPasswordConfirmed()
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = await CreateTestUserWithoutPasswordAsync(false);
            string requestUri = ApiRoutes.PasswordController.BaseUri + $"users/{user.Id}/password";

            var request = new SetPasswordRequest
            {
                Password = Password,
                PasswordConfirmed = "non matching password"
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var response = await client.PutAsync(requestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

            Assert.NotNull(errorResponse);
            Assert.NotEmpty(errorResponse.Errors);

            await CleanUpTestUserAsync(user.Email);
        }

        /// <summary>
        ///     Verifies that the <see cref="PasswordController.SetPasswordAsync"/> method returns an OK response (200) 
        ///     when the user is found and their password hash is null.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task SetPasswordAsync_ReturnsOk_WhenUserIsFoundAndPasswordHashIsNull()
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = await CreateTestUserWithoutPasswordAsync(false);
            string requestUri = ApiRoutes.PasswordController.BaseUri + $"users/{user.Id}/password";
            var jsonContent = SetupJsonPayloadContentSetPasswordRequest();

            // Act
            var response = await client.PutAsync(requestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await CleanUpTestUserAsync(user.Email);
        }

        /// <summary>
        ///     Verifies that the <see cref="PasswordController.UpdatePasswordAsync"/> method returns an Unauthorized response (401) 
        ///     when the user is not authenticated.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task UpdatePasswordAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var client = _factory.CreateClient();
            string requestUri = ApiRoutes.PasswordController.BaseUri + $"users/valid-user-id-123/password";
            var jsonContent = SetupJsonPayloadContentUpdatePasswordRequest();

            // Act
            var response = await client.PatchAsync(requestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        ///     Verifies that the <see cref="PasswordController.UpdatePasswordAsync"/> method returns a Forbidden response (403) 
        ///     when a user attempts to update another user's password without sufficient permissions.
        /// </summary>
        /// <param name="requesterRole">
        ///     The role of the user making the password update request.
        /// </param>
        /// <param name="targetRole">
        ///     The role of the user whose password is being updated.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Theory]
        [InlineData(Roles.User, Roles.User)] // User trying to update User's password
        [InlineData(Roles.User, Roles.Admin)] // User trying to update Admin's password
        [InlineData(Roles.User, Roles.SuperAdmin)] // User trying to update SuperAdmin's password
        [InlineData(Roles.Admin, Roles.Admin)] // Admin trying to update Admin's password
        [InlineData(Roles.Admin, Roles.SuperAdmin)] // Admin trying to update SuperAdmin's password
        public async Task UpdatePasswordAsync_ReturnsForbidden_WhenUserTriesToUpdateAnotherUserPasswordWithoutPermissions(string requesterRole, string targetRole)
        {
            // Arrange
            var client = _factory.CreateClient();
            var requester = await CreateTestUserWithPasswordAsync(true, requesterRole);
            var target = await CreateTestUserWithPasswordAsync(true, targetRole);

            AuthenticateClient(client, requesterRole, requester.UserName, requester.Id);

            string requestUri = ApiRoutes.PasswordController.BaseUri + $"users/{target.Id}/password";
            var jsonContent = SetupJsonPayloadContentUpdatePasswordRequest();

            // Act
            var response = await client.PatchAsync(requestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            await CleanUpTestUserAsync(requester.Email);
            await CleanUpTestUserAsync(target.Email);
        }

        /// <summary>
        ///     Verifies that the <see cref="PasswordController.UpdatePasswordAsync"/> method returns a Forbidden response (403) 
        ///     when an authenticated user attempts to update the password of a non-existent user.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task UpdatePasswordAsync_ReturnsForbidden_WhenUserDoesNotExist()
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = await CreateTestUserWithPasswordAsync(true, Roles.Admin);

            AuthenticateClient(client, Roles.Admin, user.UserName, user.Id);

            string requestUri = ApiRoutes.PasswordController.BaseUri + "users/nonexistent-user-id/password";
            var jsonContent = SetupJsonPayloadContentUpdatePasswordRequest();

            // Act
            var response = await client.PatchAsync(requestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            await CleanUpTestUserAsync(user.Email);
        }

        /// <summary>
        ///     Verifies that the <see cref="PasswordController.UpdatePasswordAsync"/> method returns a Bad Request response (400) 
        ///     when the user's password hash is null.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task UpdatePasswordAsync_ReturnsBadRequest_WhenUsersPasswordHashIsNull()
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = await CreateTestUserWithoutPasswordAsync(true, Roles.Admin);

            AuthenticateClient(client, Roles.Admin, user.UserName, user.Id);

            string requestUri = ApiRoutes.PasswordController.BaseUri + $"users/{user.Id}/password";
            var jsonContent = SetupJsonPayloadContentUpdatePasswordRequest();

            // Act
            var response = await client.PatchAsync(requestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

            Assert.NotNull(errorResponse);
            Assert.NotEmpty(errorResponse.Errors);

            await CleanUpTestUserAsync(user.Email);
        }

        /// <summary>
        ///     Verifies that the <see cref="PasswordController.UpdatePasswordAsync"/> method returns a Bad Request response (400) 
        ///     when the provided current password in the request body does not match the user's actual password.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task UpdatePasswordAsync_ReturnsBadRequest_WhenRequestBodyPasswordDoesNotMatchUserPassword()
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = await CreateTestUserWithPasswordAsync(true, Roles.Admin);

            AuthenticateClient(client, Roles.Admin, user.UserName, user.Id);

            string requestUri = ApiRoutes.PasswordController.BaseUri + $"users/{user.Id}/password";

            var request = new UpdatePasswordRequest
            {
                CurrentPassword = "incorrect password",
                NewPassword = "P_abg_123@"
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var response = await client.PatchAsync(requestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

            Assert.NotNull(errorResponse);
            Assert.NotEmpty(errorResponse.Errors);

            await CleanUpTestUserAsync(user.Email);
        }

        /// <summary>
        ///     Verifies that the <see cref="PasswordController.UpdatePasswordAsync"/> method returns a Bad Request response (400) 
        ///     when the new password provided in the request has already been used by the user.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task UpdatePasswordAsync_ReturnsBadRequest_WhenRequestedNewPasswordHasAlreadyBeenUsed()
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = await CreateTestUserWithPasswordAsync(true, Roles.Admin);

            AuthenticateClient(client, Roles.Admin, user.UserName, user.Id);

            string requestUri = ApiRoutes.PasswordController.BaseUri + $"users/{user.Id}/password";

            var request = new UpdatePasswordRequest
            {
                CurrentPassword = Password,
                NewPassword = Password
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var response = await client.PatchAsync(requestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

            Assert.NotNull(errorResponse);
            Assert.NotEmpty(errorResponse.Errors);

            await CleanUpTestUserAsync(user.Email);
        }

        /// <summary>
        ///     Verifies that the <see cref="PasswordController.UpdatePasswordAsync"/> method returns a Bad Request response (400) 
        ///     when the new password provided in the request does not meet the password requirements (e.g., length, complexity).
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task UpdatePasswordAsync_ReturnsBadRequest_WhenRequestedNewPasswordDoesNotMeetPasswordRequirements()
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = await CreateTestUserWithPasswordAsync(true, Roles.Admin);

            AuthenticateClient(client, Roles.Admin, user.UserName, user.Id);

            string requestUri = ApiRoutes.PasswordController.BaseUri + $"users/{user.Id}/password";

            var request = new UpdatePasswordRequest
            {
                CurrentPassword = Password,
                NewPassword = "password1"
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var response = await client.PatchAsync(requestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

            Assert.NotNull(errorResponse);
            Assert.NotEmpty(errorResponse.Errors);

            await CleanUpTestUserAsync(user.Email);
        }

        /// <summary>
        ///     Verifies that the <see cref="PasswordController.UpdatePasswordAsync"/> method returns an OK response (200) 
        ///     when the requested new password meets the required password requirements (e.g., length, complexity).
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task UpdatePasswordAsync_ReturnsOK_WhenRequestedNewPasswordMeetsPasswordRequirements()
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = await CreateTestUserWithPasswordAsync(true, Roles.Admin);

            AuthenticateClient(client, Roles.Admin, user.UserName, user.Id);

            string requestUri = ApiRoutes.PasswordController.BaseUri + $"users/{user.Id}/password";
            var jsonContent = SetupJsonPayloadContentUpdatePasswordRequest();

            // Act
            var response = await client.PatchAsync(requestUri, jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await CleanUpTestUserAsync(user.Email);
        }

        private static StringContent SetupJsonPayloadContentSetPasswordRequest()
        {
            var request = new SetPasswordRequest
            {
                Password = Password,
                PasswordConfirmed = Password
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"
            );
            return jsonContent;
        }

        private static StringContent SetupJsonPayloadContentUpdatePasswordRequest()
        {
            var request = new UpdatePasswordRequest
            {
                CurrentPassword = Password,
                NewPassword = "P_abg_123@"
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"
            );
            return jsonContent;
        }

        private static void AuthenticateClient(HttpClient client, string role, string userName, string userId)
        {
            var token = JwtTokenTestHelper.GenerateJwtToken(roles: new List<string> { role }, userName, userId);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task<User> CreateTestUserWithPasswordAsync(bool status, string role = null)
        {
            var createTestUserHelper = CreateTestUserHelper();

            GenerateFakeUserData(out string userName, out string email, out string firstName, out string lastName, out string phoneNumber);

            return await createTestUserHelper.CreateTestUserWithPasswordAsync(userName, firstName, lastName, email, phoneNumber, Password, status, role);
        }

        private async Task<User> CreateTestUserWithoutPasswordAsync(bool status, string role = null)
        {
            var createTestUserHelper = CreateTestUserHelper();

            GenerateFakeUserData(out string userName, out string email, out string firstName, out string lastName, out string phoneNumber);

            return await createTestUserHelper.CreateTestUserWithoutPasswordAsync(userName, firstName, lastName, email, phoneNumber, status, role);
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

        private static void GenerateFakeUserData(out string userName, out string email, out string firstName, out string lastName, out string phoneNumber)
        {
            var faker = new Faker();
            userName = faker.Internet.UserName();
            email = faker.Internet.Email();
            firstName = faker.Name.FirstName();
            lastName = faker.Name.LastName();
            phoneNumber = faker.Phone.PhoneNumber();
        }
    }
}
