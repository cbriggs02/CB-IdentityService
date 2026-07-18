using IdentityServiceApi.Features.Authorization.Interfaces;
using IdentityServiceApi.Features.Authorization.Services;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Shared.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace IdentityServiceApi.UnitTests.Features.Authorization.RoleServiceTests
{
    [Trait("Category", "Unit")]
    public partial class RoleServiceTests
    {
        private readonly Mock<IMemoryCache> _cache = new();
        private readonly Mock<RoleManager<IdentityRole>> _roleManager = RoleManagerMock();
        private readonly Mock<UserManager<User>> _userManager = UserManagerMock();
        private readonly Mock<IParameterValidator> _validator = new();
        private readonly Mock<IRoleResultFactory> _factory = new();
        private readonly Mock<IUserLookupService> _userLookup = new();
        private readonly Mock<ILoggerService> _logger = new();
        private readonly RoleService _service;

        public RoleServiceTests()
        {
            _service = new RoleService(
                _cache.Object,
                _roleManager.Object,
                _userManager.Object,
                _validator.Object,
                _factory.Object,
                _userLookup.Object,
                _logger.Object);
        }

        private static Mock<UserManager<User>> UserManagerMock()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                store.Object,
                null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private static Mock<RoleManager<IdentityRole>> RoleManagerMock()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(
                store.Object,
                null!, null!, null!, null!);
        }
    }
}