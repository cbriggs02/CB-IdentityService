using IdentityServiceApi.Features.Authorization.Services;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Shared.Context;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;

namespace IdentityServiceApi.UnitTests.Features.Authorization.AuthorizationTests
{
    [Trait("Category", "Unit")]
    public partial class AuthorizationServiceTests
    {
        private readonly Mock<UserManager<User>> _userManager = UserManagerMock();
        private readonly Mock<IUserContextService> _context = new();
        private readonly Mock<IUserLookupService> _userLookup = new();
        private readonly AuthorizationService _service;

        public AuthorizationServiceTests()
        {
            _service = new AuthorizationService(
                _userManager.Object,
                _context.Object,
                _userLookup.Object);
        }

        [Fact]
        public async Task ValidatePermissionAsync_InvalidId_ReturnsFalse()
        {
            var result = await _service.ValidatePermissionAsync("");
            Assert.False(result);
        }

        [Fact]
        public async Task ValidatePermissionAsync_NoPrincipal_ReturnsFalse()
        {
            _context.Setup(x => x.GetClaimsPrincipal()).Returns((ClaimsPrincipal)null);
            var result = await _service.ValidatePermissionAsync("123");
            Assert.False(result);
        }

        [Fact]
        public async Task ValidatePermissionAsync_NoUserId_ReturnsFalse()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity());

            _context.Setup(x => x.GetClaimsPrincipal()).Returns(principal);
            _context.Setup(x => x.GetUserId(principal)).Returns((string)null);

            var result = await _service.ValidatePermissionAsync("123");
            Assert.False(result);
        }

        [Fact]
        public async Task ValidatePermissionAsync_NoRoles_ReturnsFalse()
        {
            var principal = CreatePrincipal("1");

            SetupContext(principal);
            _context.Setup(x => x.GetRoles(principal)).Returns([]);

            var result = await _service.ValidatePermissionAsync("123");
            Assert.False(result);
        }

        private static ClaimsPrincipal CreatePrincipal(string userId, params string[] roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId)
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));
        }

        private void SetupContext(ClaimsPrincipal principal)
        {
            _context.Setup(x => x.GetClaimsPrincipal()).Returns(principal);
            _context.Setup(x => x.GetUserId(principal)).Returns(principal.FindFirstValue(ClaimTypes.NameIdentifier));
            _context.Setup(x => x.GetRoles(principal)).Returns(principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList());
        }

        private static Mock<UserManager<User>> UserManagerMock()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                store.Object,
                null!, null!, null!, null!, null!, null!, null!, null!);
        }
    }
}

