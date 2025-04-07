using IdentityServiceApi.Tests.Integration.Constants;
using IdentityServiceApi.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using IdentityServiceApi.Tests.Integration.Helpers;
using System.Net.Http.Headers;
using IdentityServiceApi.Constants;
using Newtonsoft.Json;
using IdentityServiceApi.Models.ApiResponseModels.RolesResponses;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Bogus;
using IdentityServiceApi.Data;
using IdentityServiceApi.Models.Entities;
using Bogus.DataSets;
using IdentityServiceApi.Models.ApiResponseModels.Shared;

namespace IdentityServiceApi.Tests.Integration.Controllers
{
	/// <summary>
	///     Unit tests for the <see cref="RolesController"/> class.
	///     This class contains test cases for various roles controller HTTP/HTTPS scenarios, verifying the 
	///     behavior of the roles controller functionality.
	/// </summary>
	/// <remarks>
	///     @Author: Christian Briglio
	///     @Created: 2025
	/// </remarks>
	[Trait("TestCategory", "IntegrationTest")]
	public class RolesControllerTests : IClassFixture<WebApplicationFactory<Program>>
	{
		private readonly WebApplicationFactory<Program> _factory;

		/// <summary>
		///     Initializes a new instance of the <see cref="RolesControllerTests"/> class.
		///     This constructor sets up the test environment using a <see cref="WebApplicationFactory{TEntryPoint}"/> 
		///     to create a test server for the application.
		/// </summary>
		/// <param name="factory">
		///     The <see cref="WebApplicationFactory{Program}"/> instance used to create the test server.
		/// </param>
		public RolesControllerTests(WebApplicationFactory<Program> factory)
		{
			_factory = factory;
		}

		/// <summary>
		///     Verifies that the <see cref="RolesController.GetRolesAsync"/> method returns an Unauthorized response (401) 
		///     when the user is not authenticated.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task GetRolesAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
		{
			// Arrange
			var client = _factory.CreateClient();

			// Act
			var response = await client.GetAsync(ApiRoutes.RolesController.BaseUri);

			// Assert
			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="RolesController.GetRolesAsync"/> method returns a Forbidden response (403) 
		///     when the user has insufficient privileges.
		/// </summary>
		/// <param name="roleName">
		///     The role assigned to the user making the request.
		/// </param>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Theory]
		[InlineData(Roles.Admin)]
		[InlineData(Roles.User)]
		public async Task GetRolesAsync_ReturnsForbidden_WhenUserHasInsufficientPrivileges(string roleName)
		{
			// Arrange
			var client = _factory.CreateClient();
			AuthenticateClient(client, roleName, "user123", "id-123");

			// Act
			var response = await client.GetAsync(ApiRoutes.RolesController.BaseUri);

			// Assert
			Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
		}

		/// <summary>
		///     Verifies that the <see cref="RolesController.GetRolesAsync"/> method returns an OK response (200) 
		///     when data exists and the user is authenticated with the correct privileges.
		/// </summary>
		/// <returns>
		///     A task that represents the asynchronous unit test operation.
		/// </returns>
		[Fact]
		public async Task GetRolesAsync_ReturnsOK_WhenDataExistAndUserIsAuthenticatedWithCorrectPrivileges()
		{
			// Arrange
			var client = _factory.CreateClient();
			AuthenticateClient(client, Roles.SuperAdmin, "user123", "id-123");

			// Act
			var response = await client.GetAsync(ApiRoutes.RolesController.BaseUri);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);

			var responseBody = await response.Content.ReadAsStringAsync();
			var getRolesResponse = JsonConvert.DeserializeObject<RolesListResponse>(responseBody);

			Assert.NotNull(getRolesResponse);
			Assert.NotEmpty(getRolesResponse.Roles);
		}

        /// <summary>
        ///     Verifies that the <see cref="RolesController.AssignRoleAsync(string, string)"/> method returns an 
		///     Unauthorized (401) response when the request is made without authentication.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
		public async Task AssignRoleAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
		{
			// Arrange
			var client = _factory.CreateClient();
			var jsonBody = CreateJsonPayload(Roles.User);
			string RequestUri = ApiRoutes.RolesController.BaseUri + "users/id/roles";

			// Act
			var response = await client.PostAsync(RequestUri, jsonBody);

			// Assert
			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

        /// <summary>
        ///     Verifies that the <see cref="RolesController.AssignRoleAsync(string, string)"/> method returns a 
		///     Forbidden (403) response when the authenticated user does not have sufficient privileges to assign a role.
        /// </summary>
        /// <param name="userRole">
        ///     The role of the authenticated user making the request (e.g., Admin, User).
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Theory]
		[InlineData(Roles.Admin)]
		[InlineData(Roles.User)]
		public async Task AssignRoleAsync_ReturnsForbidden_WhenUserHasInsufficientPrivileges(string userRole)
		{
			// Arrange
			var client = _factory.CreateClient();
			AuthenticateClient(client, userRole, "user123", "id-123");

			var jsonBody = CreateJsonPayload(Roles.User);
			string RequestUri = ApiRoutes.RolesController.BaseUri + "users/id-123/roles";

			// Act
			var response = await client.PostAsync(RequestUri, jsonBody);

			// Assert
			Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
		}

        /// <summary>
        ///     Verifies that the <see cref="RolesController.AssignRoleAsync(string, string)"/> method returns a 
		///     NotFound (404) response when the target user does not exist in the system.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
		public async Task AssignRoleAsync_ReturnsNotFound_WhenUserDoesNotExist()
		{
			// Arrange
			var client = _factory.CreateClient();
			AuthenticateClient(client, Roles.SuperAdmin, "user123", "id-123");

			var jsonBody = CreateJsonPayload(Roles.User);
			string RequestUri = ApiRoutes.RolesController.BaseUri + "users/id-123/roles";

			// Act
			var response = await client.PostAsync(RequestUri, jsonBody);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}

        /// <summary>
        ///     Verifies that the <see cref="RolesController.AssignRoleAsync(string, string)"/> method returns a 
		///     BadRequest (400) response when the target user exists but is not activated.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task AssignRoleAsync_ReturnsBadRequest_WhenUserIsNotActivated()
        {
            // Arrange
            var client = _factory.CreateClient();
			var requester = await CreateTestUserWithPasswordAsync(true, Roles.SuperAdmin);

            AuthenticateClient(client, Roles.SuperAdmin, requester.UserName, requester.Id);

            var target = await CreateTestUserWithPasswordAsync(false);

            var jsonBody = CreateJsonPayload(Roles.User);
            string RequestUri = ApiRoutes.RolesController.BaseUri + $"users/{target.Id}/roles";

            // Act
            var response = await client.PostAsync(RequestUri, jsonBody);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

            Assert.NotNull(errorResponse);
            Assert.NotEmpty(errorResponse.Errors);

            await CleanUpTestUserAsync(requester.Email);
            await CleanUpTestUserAsync(target.Email);
        }

        /// <summary>
        ///     Verifies that the <see cref="RolesController.AssignRoleAsync(string, string)"/> method returns a 
		///     BadRequest (400) response when the specified role does not exist in the system.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task AssignRoleAsync_ReturnsBadRequest_WhenRequestedRoleDoesNotExist()
        {
            // Arrange
            var client = _factory.CreateClient();
            var requester = await CreateTestUserWithPasswordAsync(true, Roles.SuperAdmin);

            AuthenticateClient(client, Roles.SuperAdmin, requester.UserName, requester.Id);

            var target = await CreateTestUserWithPasswordAsync(true);

            var jsonBody = CreateJsonPayload("nonexistent-role");
            string RequestUri = ApiRoutes.RolesController.BaseUri + $"users/{target.Id}/roles";

            // Act
            var response = await client.PostAsync(RequestUri, jsonBody);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

            Assert.NotNull(errorResponse);
            Assert.NotEmpty(errorResponse.Errors);

            await CleanUpTestUserAsync(requester.Email);
            await CleanUpTestUserAsync(target.Email);
        }

        /// <summary>
        ///     Verifies that the <see cref="RolesController.AssignRoleAsync(string, string)"/> method returns a 
		///     BadRequest (400) response when the target user already has the role being assigned.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task AssignRoleAsync_ReturnsBadRequest_WhenTargetUserAlreadyHasRole()
        {
            // Arrange
            var client = _factory.CreateClient();
            var requester = await CreateTestUserWithPasswordAsync(true, Roles.SuperAdmin);

            AuthenticateClient(client, Roles.SuperAdmin, requester.UserName, requester.Id);

            var target = await CreateTestUserWithPasswordAsync(true, Roles.User);

            var jsonBody = CreateJsonPayload(Roles.User);
            string RequestUri = ApiRoutes.RolesController.BaseUri + $"users/{target.Id}/roles";

            // Act
            var response = await client.PostAsync(RequestUri, jsonBody);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

            Assert.NotNull(errorResponse);
            Assert.NotEmpty(errorResponse.Errors);

            await CleanUpTestUserAsync(requester.Email);
            await CleanUpTestUserAsync(target.Email);
        }

        /// <summary>
        ///     Verifies that the <see cref="RolesController.AssignRoleAsync(string, string)"/> method returns 
		///     an OK (200) response when the role is successfully assigned to a user who does not already have it.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task AssignRoleAsync_ReturnsOK_WhenTargetUserDoesNotHaveRoleAlready()
        {
            // Arrange
            var client = _factory.CreateClient();
            var requester = await CreateTestUserWithPasswordAsync(true, Roles.SuperAdmin);

            AuthenticateClient(client, Roles.SuperAdmin, requester.UserName, requester.Id);

            var target = await CreateTestUserWithPasswordAsync(true);

            var jsonBody = CreateJsonPayload(Roles.User);
            string RequestUri = ApiRoutes.RolesController.BaseUri + $"users/{target.Id}/roles";

            // Act
            var response = await client.PostAsync(RequestUri, jsonBody);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await CleanUpTestUserAsync(requester.Email);
            await CleanUpTestUserAsync(target.Email);
        }

        private static StringContent CreateJsonPayload(string roleName)
		{
			return new StringContent(
							JsonConvert.SerializeObject(roleName),
							Encoding.UTF8,
							"application/json"
						);
		}

		private static void AuthenticateClient(HttpClient client, string role, string userName, string userId)
		{
			var token = JwtTokenTestHelper.GenerateJwtToken(roles: new List<string> { role }, userName, userId);
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}

		private async Task<User> CreateTestUserWithPasswordAsync(bool status, string role = null)
		{
			var createTestUserHelper = CreateTestUserHelper();

            var faker = new Faker();
            string userName = faker.Internet.UserName();
            string email = faker.Internet.Email();
            string firstName = faker.Name.FirstName();
            string lastName = faker.Name.LastName();
            string phoneNumber = faker.Phone.PhoneNumber();

            return await createTestUserHelper.CreateTestUserWithPasswordAsync(userName, firstName, lastName, email, phoneNumber, "Test@1234", status, role);
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
