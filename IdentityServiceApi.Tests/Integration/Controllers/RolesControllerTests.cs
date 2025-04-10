using IdentityServiceApi.Tests.Integration.Constants;
using IdentityServiceApi.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using IdentityServiceApi.Tests.Integration.Helpers;
using System.Net.Http.Headers;
using IdentityServiceApi.Constants;
using Newtonsoft.Json;
using IdentityServiceApi.Models.ApiResponseModels.RolesResponses;

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

		private static void AuthenticateClient(HttpClient client, string role, string userName, string userId)
		{
			var token = JwtTokenTestHelper.GenerateJwtToken(roles: new List<string> { role }, userName, userId);
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}
	}
}
