using Bogus;
using IdentityServiceApi.Constants;
using IdentityServiceApi.Controllers;
using IdentityServiceApi.Data;
using IdentityServiceApi.Models.ApiResponseModels.Roles;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Tests.Integration.Constants;
using IdentityServiceApi.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;

namespace IdentityServiceApi.Tests.Integration.Controllers
{
    /// <summary>
    ///     Integration tests for the <see cref="RolesController"/> class.
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
            var response = await client.GetAsync($"{ApiRoutes.RolesController.RequestUri}/");

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
            var user = await CreateTestUserWithPasswordAsync(true, roleName);

            AuthenticateClient(client, roleName, user.UserName, user.Id);

            // Act
            var response = await client.GetAsync($"{ApiRoutes.RolesController.RequestUri}/");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            await CleanUpTestUserAsync(user.Email);
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
            var user = await CreateTestUserWithPasswordAsync(true, Roles.SuperAdmin);

            AuthenticateClient(client, Roles.SuperAdmin, user.UserName, user.Id);

            // Act
            var response = await client.GetAsync($"{ApiRoutes.RolesController.RequestUri}/");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var getRolesResponse = JsonConvert.DeserializeObject<RolesListResponse>(responseBody);

            Assert.NotNull(getRolesResponse);
            Assert.NotEmpty(getRolesResponse.Roles);

            await CleanUpTestUserAsync(user.Email);
        }

        /// <summary>
        ///     Verifies that the <see cref="RolesController.GetRoleAsync"/> method returns an Unauthorized (401)
        ///     status code when the request is made without user authentication.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task GetRoleAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var client = _factory.CreateClient();
            const string RequestUri = $"{ApiRoutes.RolesController.RequestUri}/role-id";

            // Act
            var response = await client.GetAsync(RequestUri);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        ///     Verifies that the <see cref="RolesController.GetRoleAsync"/> method returns a Forbidden (403)
        ///     status code when an authenticated user lacks sufficient privileges to access the endpoint.
        /// </summary>
        /// <param name="roleName">
        ///     The role assigned to the test user making the request (e.g., Admin, User).
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Theory]
        [InlineData(Roles.Admin)]
        [InlineData(Roles.User)]
        public async Task GetRoleAsync_ReturnsForbidden_WhenUserHasInsufficientPrivileges(string roleName)
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = await CreateTestUserWithPasswordAsync(true, roleName);

            AuthenticateClient(client, roleName, user.UserName, user.Id);

            const string RequestUri = $"{ApiRoutes.RolesController.RequestUri}/role-id";

            // Act
            var response = await client.GetAsync(RequestUri);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            await CleanUpTestUserAsync(user.Email);
        }

        /// <summary>
        ///     Verifies that the <see cref="RolesController.GetRoleAsync"/> method returns a NotFound (404)
        ///     status code when the specified role does not exist in the system.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task GetRoleAsync_ReturnsNotFound_WhenRoleDoesNotExist()
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = await CreateTestUserWithPasswordAsync(true, Roles.SuperAdmin);

            AuthenticateClient(client, Roles.SuperAdmin, user.UserName, user.Id);

            const string RequestUri = $"{ApiRoutes.RolesController.RequestUri}/nonexistent-role-id";

            // Act
            var response = await client.GetAsync(RequestUri);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await CleanUpTestUserAsync(user.Email);
        }

        /// <summary>
        ///     Verifies that the <see cref="RolesController.GetRoleAsync"/> method returns an OK (200)
        ///     status code along with valid role data when the requested role exists and the user
        ///     is authenticated with appropriate privileges.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous unit test operation.
        /// </returns>
        [Fact]
        public async Task GetRoleAsync_ReturnsOK_WhenRoleExistsAndUserIsAuthenticatedWithCorrectRole()
        {
            // Arrange
            var client = _factory.CreateClient();
            var user = await CreateTestUserWithPasswordAsync(true, Roles.SuperAdmin);

            AuthenticateClient(client, Roles.SuperAdmin, user.UserName, user.Id);

            var role = await CreateTestRoleDataAsync();
            string RequestUri = $"{ApiRoutes.RolesController.RequestUri}/{role.Id}";

            // Act
            var response = await client.GetAsync(RequestUri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var getRoleResponse = JsonConvert.DeserializeObject<RoleResponse>(responseBody);

            Assert.NotNull(getRoleResponse);
            Assert.NotNull(getRoleResponse.Role);

            await CleanUpTestUserAsync(user.Email);
            await CleanupTestRoleDataAsync(role);
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

            return await createTestUserHelper.CreateTestUserWithPasswordAsync(userName, firstName, lastName, email, phoneNumber, "Test@1234", status, role);
        }

        private async Task CleanUpTestUserAsync(string email)
        {
            var createTestUserHelper = CreateTestUserHelper();
            await createTestUserHelper.DeleteTestUserAsync(email);
        }

        private CreateTestUserHelper CreateTestUserHelper()
        {
            var serviceProvider = GetServiceProvider();
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

        private async Task<IdentityRole> CreateTestRoleDataAsync()
        {
            const string RoleName = "test-role";
            var roleManager = GetRoleManager();
            await roleManager.CreateAsync(new IdentityRole(RoleName));

            var role = await roleManager.FindByNameAsync(RoleName);
            return role ?? throw new InvalidOperationException("Failed to find role");
        }

        private async Task CleanupTestRoleDataAsync(IdentityRole role)
        {
            var roleManager = GetRoleManager();
            var result = await roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Failed to delete role.");
            }
        }

        private RoleManager<IdentityRole> GetRoleManager()
        {
            var serviceProvider = GetServiceProvider();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            return roleManager;
        }

        private IServiceProvider GetServiceProvider()
        {
            var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            return serviceProvider;
        }
    }
}
