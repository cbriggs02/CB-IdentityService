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
using IdentityServiceApi.Models.ApiResponseModels.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

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

        private async Task CleanupRoleCreatedInTest(string roleName)
        {
            var roleManager = GetRoleManagerService();

            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return;
            }

            var result = await roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to delete role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        private async Task<string> CreateRoleForTest(string roleName)
        {
            var roleManager = GetRoleManagerService();

            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create test role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                throw new InvalidOperationException($"Failed to find role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return role.Id;
        }

        private RoleManager<IdentityRole> GetRoleManagerService()
        {
            var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            return roleManager;
        }
    }
}
