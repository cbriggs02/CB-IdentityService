using IdentityServiceApi.Constants;
using IdentityServiceApi.Services.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;
using System.Security.Claims;

namespace IdentityServiceApi.Tests.Unit.Services.Authentication
{
    /// <summary>
    ///     Unit tests for the <see cref="UserContextService"/> class.
    ///     This class contains tests to validate the functionality of methods 
    ///     in the UserContextService, ensuring correct behavior under various scenarios.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    [Trait("TestCategory", "UnitTest")]
    public class UserContextServiceTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly UserContextService _userContextService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserContextServiceTests"/> class.
        ///     This constructor sets up the necessary mocks and initializes the UserContextService instance
        ///     to be used in the unit tests.
        /// </summary>
        public UserContextServiceTests()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _userContextService = new UserContextService(_httpContextAccessorMock.Object);
        }

        /// <summary>
        ///     Tests that an <see cref="ArgumentNullException"/> is thrown when <see cref="UserContextService"/> is 
        ///     instantiated with a null dependencies.
        /// </summary>
        [Fact]
        public void UserContextService_NullDependencies_ThrowsArgumentNullException()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UserContextService(null));
        }

        /// <summary>
        ///     Tests that <see cref="UserContextService.GetClaimsPrincipal"/> returns a valid 
        ///     <see cref="ClaimsPrincipal"/> when the user is authenticated.
        ///     This test simulates an authenticated user context and verifies that the 
        ///     returned claims principal has the expected user name.
        /// </summary>
        [Fact]
        public void GetClaimsPrincipal_ShouldReturnClaimsPrincipal_WhenUserIsAuthenticated()
        {
            // Arrange
            const string userName = "TestUser";

            var httpContext = CreateHttpContextWithUser(userName);

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            // Act
            var result = _userContextService.GetClaimsPrincipal();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userName, result.Identity.Name);
        }

        /// <summary>
        ///     Verifies that <see cref="UserContextService.GetClaimsPrincipal"/> returns null
        ///     when the HttpContext is null, simulating cases with no active HTTP request.
        /// </summary>
        [Fact]
        public void GetClaimsPrincipal_ShouldReturnNull_WhenHttpContextIsNull()
        {
            // Arrange
            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns((HttpContext)null);

            // Act
            var result = _userContextService.GetClaimsPrincipal();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        ///     Tests that <see cref="UserContextService.GetUserId"/> returns the correct 
        ///     user ID when the user is authenticated.
        ///     This test ensures that the method correctly retrieves the user ID from the 
        ///     provided claims principal.
        /// </summary>
        [Fact]
        public void GetUserID_ShouldReturnUserId_WhenUserIsAuthenticated()
        {
            // Arrange
            const string expectedUserId = "12345";

            var claimsPrincipal = CreateClaimsPrincipalWithUserId(expectedUserId);

            // Act
            var result = _userContextService.GetUserId(claimsPrincipal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedUserId, result);
        }

        /// <summary>
        ///     Tests that <see cref="UserContextService.GetUserId"/> returns null when the 
        ///     provided <see cref="ClaimsPrincipal"/> is null.
        ///     This test verifies that the method handles null input gracefully, ensuring 
        ///     no exceptions are thrown and the expected null result is returned.
        /// </summary>
        [Fact]
        public void GetUserID_ShouldReturnNullUserId_WhenClaimsPrincipalIsNull()
        {
            // Act
            var result = _userContextService.GetUserId(null);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        ///     Tests that <see cref="UserContextService.GetRoles"/> returns a list of roles 
        ///     when the user is authenticated.
        ///     This test verifies that the method correctly retrieves all roles associated 
        ///     with the provided claims principal.
        /// </summary>
        [Fact]
        public void GetRoles_ShouldReturnRoles_WhenUserIsAuthenticated()
        {
            // Arrange
            List<string> roles = new() { Roles.User, Roles.Admin };

            var claimsPrincipal = CreateClaimsPrinciplesWithRoles(roles);

            // Act
            var result = _userContextService.GetRoles(claimsPrincipal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(roles, result);
        }

        /// <summary>
        ///     Tests that <see cref="UserContextService.GetRoles"/> returns an empty list 
        ///     when the claims principal does not have any roles assigned.
        ///     This test verifies that the method correctly handles the case where a user 
        ///     is authenticated but does not have any associated roles, ensuring an 
        ///     empty list is returned instead of null or an error.
        /// </summary>
        [Fact]
        public void GetRoles_ShouldReturnEmptyList_WhenNoRolesAreAssigned()
        {
            // Arrange
            var claimsPrincipal = CreateClaimsPrinciplesWithRoles(new List<string>());

            // Act
            var result = _userContextService.GetRoles(claimsPrincipal);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        ///     Tests that <see cref="UserContextService.GetRoles"/> returns an empty list 
        ///     when the provided claims principal is null.
        ///     This test ensures that the method can gracefully handle null input 
        ///     without throwing exceptions and still return an empty list, 
        ///     which is important for robust error handling.
        /// </summary>
        [Fact]
        public void GetRoles_ShouldReturnEmptyList_WhenClaimsPrincipalIsNull()
        {
            // Act
            var result = _userContextService.GetRoles(null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        ///     Tests that <see cref="UserContextService.GetAddress"/> returns the correct 
        ///     IP address when the user is authenticated. This test verifies that the 
        ///     method successfully retrieves the remote IP address from the 
        ///     HTTP context's connection, ensuring that authenticated users can access 
        ///     their associated network information.
        /// </summary>
        [Fact]
        public void GetAddress_ShouldReturnAddress_WhenUserIsAuthenticated()
        {
            // Arrange
            var expectedAddress = IPAddress.Parse("127.0.0.1");

            var httpContext = new DefaultHttpContext
            {
                Connection = { RemoteIpAddress = expectedAddress }
            };

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            // Act
            var result = _userContextService.GetAddress();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedAddress, result);
        }

        /// <summary>
        ///     Tests that <see cref="UserContextService.GetAddress"/> returns null 
        ///     when the HTTP context is null. This test ensures that the method can 
        ///     handle cases where the HTTP context is not available, preventing 
        ///     null reference exceptions and maintaining stable behavior.
        /// </summary>
        [Fact]
        public void GetAddress_ShouldReturnNull_WhenHttpContextIsNull()
        {
            // Arrange
            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns((HttpContext)null);

            // Act
            var result = _userContextService.GetAddress();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        ///     Tests that <see cref="UserContextService.GetRequestPath"/> returns the 
        ///     correct request path when the user is authenticated. This test verifies 
        ///     that the method accurately retrieves the request path from the 
        ///     HTTP context, ensuring that authenticated users can access their 
        ///     request details as expected.
        /// </summary>
        [Fact]
        public void GetRequestPath_ShouldReturnRequestPath_WhenUserIsAuthenticated()
        {
            // Arrange
            const string expectedPath = "/path";

            var httpContext = new DefaultHttpContext
            {
                Request = { Path = expectedPath}
            };

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            // Act
            var result = _userContextService.GetRequestPath();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPath, result);
        }

        /// <summary>
        ///     Verifies that <see cref="UserContextService.GetRequestPath"/> returns null
        ///     when the HttpContext is null, simulating cases with no active HTTP request.
        /// </summary>
        [Fact]
        public void GetRequestPath_ShouldReturnNull_WhenHttpContextIsNull()
        {
            // Arrange
            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns((HttpContext)null);

            // Act
            var result = _userContextService.GetRequestPath();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        ///     Creates a DefaultHttpContext with the specified user's name.
        /// </summary>
        /// <param name="userName">
        ///     The name of the authenticated user.
        /// </param>
        /// <returns>
        ///     A configured DefaultHttpContext.
        /// </returns>
        private static DefaultHttpContext CreateHttpContextWithUser(string userName)
        {
            var claimsPrincipal = CreateClaimsPrincipalWithUserName(userName);

            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            return httpContext;
        }

        /// <summary>
        ///     Creates a ClaimsPrincipal with the specified user's ID.
        /// </summary>
        /// <param name="userId">
        ///     The ID of the authenticated user.
        /// </param>
        /// <returns>
        ///     A configured ClaimsPrincipal.
        /// </returns>
        private static ClaimsPrincipal CreateClaimsPrincipalWithUserId(string userId)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId)
            };
            return CreateClaimsPrincipal(claims);
        }

        /// <summary>
        ///     Creates a ClaimsPrincipal with the specified user's name.
        /// </summary>
        /// <param name="userName">
        ///     The name of the authenticated user.
        /// </param>
        /// <returns>
        ///     A configured ClaimsPrincipal.
        /// </returns>
        private static ClaimsPrincipal CreateClaimsPrincipalWithUserName(string userName)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, userName)
            };
            return CreateClaimsPrincipal(claims);
        }

        /// <summary>
        ///     Creates a ClaimsPrincipal with the specified roles for testing purposes.
        /// </summary>
        /// <param name="roles">
        ///     The list of roles to add to the claims principal.
        /// </param>
        /// <returns>
        ///     A ClaimsPrincipal containing the specified roles.
        /// </returns>
        private static ClaimsPrincipal CreateClaimsPrinciplesWithRoles(List<string> roles)
        {
            var claims = new List<Claim>();

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return CreateClaimsPrincipal(claims);
        }

        /// <summary>
        ///     Creates a ClaimsPrincipal with the given claims.
        /// </summary>
        /// <param name="claims">
        ///     A list of claims to include in the ClaimsPrincipal.
        /// </param>
        /// <returns>
        ///     A configured ClaimsPrincipal.
        /// </returns>
        private static ClaimsPrincipal CreateClaimsPrincipal(IEnumerable<Claim> claims)
        {
            const string authenticationType = "TestAuthentication";
            var identity = new ClaimsIdentity(claims, authenticationType);
            return new ClaimsPrincipal(identity);
        }
    }
}
